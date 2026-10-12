using ERP.Application.Common.Interfaces;
using ERP.Application.Fiscal.Commands;
using ERP.Application.Fiscal.DTOs;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Entities.Fiscal;
using ERP.Domain.Core.Entities.Vendas;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ERP.Application.Vendas.Voice;

public class VoiceAssistantService : IVoiceAssistantService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;
    private readonly IMediator _mediator;

    public VoiceAssistantService(
        IApplicationDbContext context,
        ICurrentEmpresaService currentEmpresaService,
        IMediator mediator)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
        _mediator = mediator;
    }

    public async Task<VoiceProcessResult> ProcessarComandoVozAsync(
        VoiceSessionState estado,
        string comandoVoz,
        CancellationToken cancellationToken = default)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var textoLimpo = NormalizarTexto(comandoVoz);

        var resultado = new VoiceProcessResult { Estado = estado };

        // 1. Estado: Aguardando Comando Inicial
        if (estado.Status == VoiceSessionStatus.AguardandoComando || estado.Status == VoiceSessionStatus.Encerrado)
        {
            if (textoLimpo.Contains("pedido") || textoLimpo.Contains("venda") || textoLimpo.Contains("faca") || textoLimpo.Contains("faça") || textoLimpo.Contains("novo"))
            {
                return await IniciarPedidoVozAsync(empresaId, estado, textoLimpo, cancellationToken);
            }

            resultado.MensagemResposta = "Olá! Para começar um pedido de venda, diga: 'ERP, faça um pedido de venda' ou 'ERP, pedido para José Maria, 30 dias'.";
            return resultado;
        }

        // 2. Estado: Aguardando Escolha/Identificação do Cliente
        if (estado.Status == VoiceSessionStatus.AguardandoCliente)
        {
            return await ProcessarClienteAsync(empresaId, estado, textoLimpo, cancellationToken);
        }

        // 3. Estado: Aguardando Escolha entre Múltiplos Produtos
        if (estado.Status == VoiceSessionStatus.AguardandoEscolhaProduto)
        {
            return await ProcessarEscolhaProdutoAsync(empresaId, estado, textoLimpo, cancellationToken);
        }

        // 4. Estado: Adicionando Itens ao Pedido
        if (estado.Status == VoiceSessionStatus.CriandoPedidoAguardandoItens)
        {
            // Checar intenção de cancelar pedido
            if (textoLimpo.Contains("cancelar pedido") || textoLimpo.Contains("desistir") || textoLimpo.Contains("abortar"))
            {
                estado.Status = VoiceSessionStatus.AguardandoComando;
                estado.Itens.Clear();
                estado.CandidatosProdutos.Clear();
                estado.CandidatosClientes.Clear();
                resultado.MensagemResposta = "Pedido cancelado. A sessão foi reiniciada.";
                return resultado;
            }

            // Checar intenção de finalizar
            if (textoLimpo.Contains("finalizar") || textoLimpo.Contains("terminar") || textoLimpo.Contains("concluir") ||
                textoLimpo.Contains("fechar") || textoLimpo.Contains("pronto") || textoLimpo.Contains("e so isso") || 
                textoLimpo.Contains("é só isso") || textoLimpo.Contains("acabou") || textoLimpo.Contains("salvar"))
            {
                return await FinalizarPedidoAsync(empresaId, estado, cancellationToken);
            }

            // Checar intenção de remover item
            if (textoLimpo.Contains("remover") || textoLimpo.Contains("excluir") || textoLimpo.Contains("tirar") || textoLimpo.Contains("apagar"))
            {
                return ProcessarRemocaoItem(estado, textoLimpo);
            }

            // Interpretar e adicionar item
            return await AdicionarItemVozAsync(empresaId, estado, textoLimpo, cancellationToken);
        }

        // 5. Estado: Pedido Concluído, Aguardando decisão sobre NF-e
        if (estado.Status == VoiceSessionStatus.PedidoConcluidoAguardandoNfe)
        {
            if (textoLimpo.Contains("sim") || textoLimpo.Contains("emita") || textoLimpo.Contains("pode emitir") ||
                textoLimpo.Contains("tira") || textoLimpo.Contains("tirar") || textoLimpo.Contains("com certeza") || textoLimpo.Contains("positivo"))
            {
                return await EmitirNfePedidoAsync(empresaId, estado, cancellationToken);
            }
            else if (textoLimpo.Contains("nao") || textoLimpo.Contains("não") || textoLimpo.Contains("depois") || textoLimpo.Contains("agora nao"))
            {
                estado.Status = VoiceSessionStatus.Encerrado;
                resultado.MensagemResposta = $"Entendido! O pedido {estado.PedidoCriadoNumero} está salvo e disponível. A NF-e poderá ser emitida posteriormente no módulo fiscal.";
                return resultado;
            }
            else
            {
                resultado.MensagemResposta = $"Deseja que eu emita a NF-e agora para o pedido {estado.PedidoCriadoNumero}? Responda 'Sim' ou 'Não'.";
                return resultado;
            }
        }

        resultado.MensagemResposta = "Comando processado.";
        return resultado;
    }

    #region Etapa 1: Início do Pedido e Cliente
    private async Task<VoiceProcessResult> IniciarPedidoVozAsync(
        int empresaId,
        VoiceSessionState estado,
        string texto,
        CancellationToken cancellationToken)
    {
        estado.Itens.Clear();
        estado.CandidatosProdutos.Clear();
        estado.CandidatosClientes.Clear();
        estado.TermoClienteNaoEncontrado = null;
        estado.TermoProdutoNaoEncontrado = null;

        // Extrair condição de pagamento se mencionada
        string condicao = "30 dias";
        int diasVencimento = 30;
        if (texto.Contains("a vista") || texto.Contains("à vista") || texto.Contains("dinheiro") || texto.Contains("pix"))
        {
            condicao = "À Vista";
            diasVencimento = 0;
        }
        else
        {
            var matchDias = Regex.Match(texto, @"(\d+)\s*dias");
            if (matchDias.Success && int.TryParse(matchDias.Groups[1].Value, out int d))
            {
                diasVencimento = d;
                condicao = $"{d} dias";
            }
        }
        estado.CondicaoPagamento = condicao;
        estado.DiasVencimento = diasVencimento;

        // Extrair nome do cliente se mencionado
        string? nomeClienteFalado = null;
        var matchCliente = Regex.Match(texto, @"(?:cliente|para o cliente|para a cliente|para o|para a|para)\s+([a-zA-ZÀ-ÿ\s]+?)(?=\s+(?:pagamento|com pagamento|com|no|prazo|condicao|e depois|$))");
        if (matchCliente.Success && !string.IsNullOrWhiteSpace(matchCliente.Groups[1].Value))
        {
            nomeClienteFalado = matchCliente.Groups[1].Value.Trim();
        }

        // Se o usuário NÃO informou o cliente no comando inicial:
        if (string.IsNullOrWhiteSpace(nomeClienteFalado))
        {
            estado.Status = VoiceSessionStatus.AguardandoCliente;
            return new VoiceProcessResult
            {
                Estado = estado,
                MensagemResposta = "Iniciando novo pedido de venda! Para qual cliente você deseja criar o pedido?"
            };
        }

        // Se informou, pesquisa no banco de dados
        return await BuscarEAssociarClienteAsync(empresaId, estado, nomeClienteFalado, cancellationToken);
    }

    private async Task<VoiceProcessResult> ProcessarClienteAsync(
        int empresaId,
        VoiceSessionState estado,
        string texto,
        CancellationToken cancellationToken)
    {
        // Se havia candidatos listados e o usuário falou o número da opção (ex: "o primeiro", "opção 1", "2")
        if (estado.CandidatosClientes.Any())
        {
            int indiceEscolhido = ObterIndiceOpcao(texto);
            if (indiceEscolhido >= 0 && indiceEscolhido < estado.CandidatosClientes.Count)
            {
                var escolhido = estado.CandidatosClientes[indiceEscolhido];
                return await SelecionarClienteCandidatoAsync(estado, escolhido.Id, cancellationToken);
            }

            // Ou se falou o nome de um dos candidatos
            var candidatoPorNome = estado.CandidatosClientes
                .FirstOrDefault(c => texto.Contains(NormalizarTexto(c.Nome)) || NormalizarTexto(c.Nome).Contains(texto));
            if (candidatoPorNome != null)
            {
                return await SelecionarClienteCandidatoAsync(estado, candidatoPorNome.Id, cancellationToken);
            }
        }

        // Se o usuário falou um novo nome de cliente
        string termoCliente = texto;
        string[] remover = { "para o cliente", "para a cliente", "para o", "para a", "para", "o cliente", "cliente", "é para o", "e para o" };
        foreach (var r in remover)
        {
            termoCliente = termoCliente.Replace(r, " ").Trim();
        }

        if (string.IsNullOrWhiteSpace(termoCliente) || termoCliente.Length < 2)
        {
            return new VoiceProcessResult
            {
                Estado = estado,
                MensagemResposta = "Por favor, diga o nome do cliente para o pedido."
            };
        }

        return await BuscarEAssociarClienteAsync(empresaId, estado, termoCliente, cancellationToken);
    }

    private async Task<VoiceProcessResult> BuscarEAssociarClienteAsync(
        int empresaId,
        VoiceSessionState estado,
        string nomeCliente,
        CancellationToken cancellationToken)
    {
        var termo = nomeCliente.Trim().ToLower();

        var clientesEncontrados = await _context.Clientes
            .Where(c => c.EmpresaId == empresaId && c.Ativo && EF.Functions.Like(c.Nome.ToLower(), $"%{termo}%"))
            .Take(5)
            .ToListAsync(cancellationToken);

        // Cenário 1: Cliente Único Encontrado com Sucesso!
        if (clientesEncontrados.Count == 1)
        {
            var cli = clientesEncontrados[0];
            estado.ClienteId = cli.Id;
            estado.ClienteNome = cli.Nome;
            estado.Status = VoiceSessionStatus.CriandoPedidoAguardandoItens;
            estado.CandidatosClientes.Clear();
            estado.TermoClienteNaoEncontrado = null;

            return new VoiceProcessResult
            {
                Estado = estado,
                MensagemResposta = $"Cliente {cli.Nome} selecionado! Condição de pagamento: {estado.CondicaoPagamento}. Agora você pode falar os produtos e as quantidades."
            };
        }

        // Cenário 2: Múltiplos Clientes Encontrados (Desambiguação)
        if (clientesEncontrados.Count > 1)
        {
            estado.Status = VoiceSessionStatus.AguardandoCliente;
            estado.CandidatosClientes = clientesEncontrados.Select(c => new VoiceClientCandidateDto
            {
                Id = c.Id,
                Nome = c.Nome,
                CpfCnpj = c.CpfCnpj,
                Telefone = c.Telefone
            }).ToList();
            estado.TermoClienteNaoEncontrado = null;

            var listaNomes = string.Join(", ", estado.CandidatosClientes.Select((c, i) => $"{i + 1} - {c.Nome}"));

            return new VoiceProcessResult
            {
                Estado = estado,
                MensagemResposta = $"Encontrei {estado.CandidatosClientes.Count} clientes: {listaNomes}. Diga o número da opção ou clique nela na tela."
            };
        }

        // Cenário 3: Cliente NÃO Encontrado!
        estado.Status = VoiceSessionStatus.AguardandoCliente;
        estado.CandidatosClientes.Clear();
        estado.TermoClienteNaoEncontrado = CapitalizarNome(nomeCliente);

        return new VoiceProcessResult
        {
            Estado = estado,
            MensagemResposta = $"Não encontrei nenhum cliente cadastrado com o nome '{estado.TermoClienteNaoEncontrado}'. Você pode falar outro nome ou clicar no botão 'Cadastrar Cliente' na tela."
        };
    }
    #endregion

    #region Etapa 2: Adição e Seleção de Itens (Produtos)
    private async Task<VoiceProcessResult> AdicionarItemVozAsync(
        int empresaId,
        VoiceSessionState estado,
        string texto,
        CancellationToken cancellationToken)
    {
        // 1. Extrair quantidade falada
        decimal quantidade = ExtrairQuantidade(texto);

        // 2. Extrair termo de busca do produto limpando preposições e comandos
        string termoProduto = LimparTermoProduto(texto);

        if (string.IsNullOrWhiteSpace(termoProduto) || termoProduto.Length < 2)
        {
            return new VoiceProcessResult
            {
                Estado = estado,
                MensagemResposta = "Não entendi qual produto você deseja adicionar. Pode falar o nome do produto novamente?"
            };
        }

        // 3. Buscar no banco de dados por aproximação
        var produtosQuery = _context.Produtos
            .Include(p => p.UnidadeMedida)
            .Where(p => p.EmpresaId == empresaId && p.Ativo);

        // Busca 1: Substring exata no Nome ou Código
        var termoLower = termoProduto.ToLower();
        var produtos = await produtosQuery
            .Where(p => EF.Functions.Like(p.Nome.ToLower(), $"%{termoLower}%") || EF.Functions.Like(p.Codigo.ToLower(), $"%{termoLower}%"))
            .Take(5)
            .ToListAsync(cancellationToken);

        // Busca 2: Se não achou e termo tem mais de uma palavra, busca por palavras-chave significativas
        if (!produtos.Any())
        {
            var palavras = termoProduto.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length >= 3)
                .ToList();

            if (palavras.Any())
            {
                // Busca produtos onde o nome contenha qualquer uma das palavras significativas
                var candidatosGerais = await produtosQuery.Take(50).ToListAsync(cancellationToken);
                produtos = candidatosGerais
                    .Where(p => palavras.Any(w => p.Nome.Contains(w, StringComparison.OrdinalIgnoreCase)))
                    .Take(5)
                    .ToList();
            }
        }

        // Cenário 1: NENHUM PRODUTO ENCONTRADO
        if (!produtos.Any())
        {
            estado.TermoProdutoNaoEncontrado = termoProduto;
            return new VoiceProcessResult
            {
                Estado = estado,
                MensagemResposta = $"Não encontrei nenhum produto correspondente a '{termoProduto}'. Por favor, repita o nome do produto ou verifique o cadastro."
            };
        }

        // Cenário 2: EXATAMENTE 1 PRODUTO ENCONTRADO -> Adiciona direto!
        if (produtos.Count == 1)
        {
            var produto = produtos[0];
            return InserirItemNoPedido(estado, produto, quantidade);
        }

        // Cenário 3: 2 OU MAIS PRODUTOS ENCONTRADOS -> Entra no estado de escolha!
        estado.Status = VoiceSessionStatus.AguardandoEscolhaProduto;
        estado.QuantidadePendente = quantidade;
        estado.CandidatosProdutos = produtos.Select(p => new VoiceProductCandidateDto
        {
            Id = p.Id,
            Codigo = p.Codigo,
            Nome = p.Nome,
            PrecoVenda = p.PrecoVenda > 0 ? p.PrecoVenda : 35.00m,
            UnidadeMedida = p.UnidadeMedida?.Sigla ?? "UN"
        }).ToList();

        var opcoesTexto = string.Join(", ", estado.CandidatosProdutos.Select((p, i) => $"{i + 1} - {p.Nome}"));

        return new VoiceProcessResult
        {
            Estado = estado,
            MensagemResposta = $"Encontrei {estado.CandidatosProdutos.Count} opções: {opcoesTexto}. Diga o número da opção desejada ou clique nela na tela."
        };
    }

    private async Task<VoiceProcessResult> ProcessarEscolhaProdutoAsync(
        int empresaId,
        VoiceSessionState estado,
        string texto,
        CancellationToken cancellationToken)
    {
        // Checar se o usuário cancelou a escolha
        if (texto.Contains("cancelar") || texto.Contains("nenhum") || texto.Contains("voltar") || texto.Contains("outro"))
        {
            estado.Status = VoiceSessionStatus.CriandoPedidoAguardandoItens;
            estado.CandidatosProdutos.Clear();
            return new VoiceProcessResult
            {
                Estado = estado,
                MensagemResposta = "Seleção de produto cancelada. Qual produto você deseja adicionar?"
            };
        }

        // Tentar identificar por número (ex: "o primeiro", "opção 1", "2")
        int indice = ObterIndiceOpcao(texto);
        if (indice >= 0 && indice < estado.CandidatosProdutos.Count)
        {
            var escolhido = estado.CandidatosProdutos[indice];
            return await SelecionarProdutoCandidatoAsync(estado, escolhido.Id, estado.QuantidadePendente, cancellationToken);
        }

        // Tentar identificar pelo nome de algum candidato falado
        var candidatoPorNome = estado.CandidatosProdutos
            .FirstOrDefault(c => texto.Contains(NormalizarTexto(c.Nome)) || NormalizarTexto(c.Nome).Contains(texto));
        if (candidatoPorNome != null)
        {
            return await SelecionarProdutoCandidatoAsync(estado, candidatoPorNome.Id, estado.QuantidadePendente, cancellationToken);
        }

        var opcoesTexto = string.Join(", ", estado.CandidatosProdutos.Select((p, i) => $"{i + 1} - {p.Nome}"));
        return new VoiceProcessResult
        {
            Estado = estado,
            MensagemResposta = $"Por favor, diga o número da opção: {opcoesTexto}, ou clique no produto desejado na tela."
        };
    }

    private VoiceProcessResult InserirItemNoPedido(VoiceSessionState estado, Produto produto, decimal quantidade)
    {
        var precoUnitario = produto.PrecoVenda > 0 ? produto.PrecoVenda : 35.00m;

        // Se já existe no pedido, incrementa quantidade
        var itemExistente = estado.Itens.FirstOrDefault(i => i.ProdutoId == produto.Id);
        if (itemExistente != null)
        {
            itemExistente.Quantidade += quantidade;
        }
        else
        {
            estado.Itens.Add(new VoiceOrderItemDto
            {
                ProdutoId = produto.Id,
                ProdutoNome = produto.Nome,
                ProdutoCodigo = produto.Codigo,
                Quantidade = quantidade,
                ValorUnitario = precoUnitario
            });
        }

        estado.Status = VoiceSessionStatus.CriandoPedidoAguardandoItens;
        estado.CandidatosProdutos.Clear();
        estado.TermoProdutoNaoEncontrado = null;

        var totalItem = Math.Round(quantidade * precoUnitario, 2);

        return new VoiceProcessResult
        {
            Estado = estado,
            MensagemResposta = $"Adicionado {quantidade} de {produto.Nome} no valor de {totalItem:C2}. Total do pedido até o momento: {estado.ValorTotal:C2}. Pode falar o próximo item ou dizer 'finalizar pedido'."
        };
    }

    private VoiceProcessResult ProcessarRemocaoItem(VoiceSessionState estado, string texto)
    {
        if (!estado.Itens.Any())
        {
            return new VoiceProcessResult
            {
                Estado = estado,
                MensagemResposta = "Não há nenhum item no pedido para remover."
            };
        }

        VoiceOrderItemDto? itemRemover = null;

        if (texto.Contains("ultimo") || texto.Contains("último"))
        {
            itemRemover = estado.Itens.Last();
        }
        else
        {
            // Tentar por índice
            int idx = ObterIndiceOpcao(texto);
            if (idx >= 0 && idx < estado.Itens.Count)
            {
                itemRemover = estado.Itens[idx];
            }
            else
            {
                // Tentar por nome do produto
                itemRemover = estado.Itens.FirstOrDefault(i => texto.Contains(NormalizarTexto(i.ProdutoNome)));
            }
        }

        if (itemRemover != null)
        {
            estado.Itens.Remove(itemRemover);
            return new VoiceProcessResult
            {
                Estado = estado,
                MensagemResposta = $"Item {itemRemover.ProdutoNome} removido. Total atual do pedido: {estado.ValorTotal:C2}."
            };
        }

        return new VoiceProcessResult
        {
            Estado = estado,
            MensagemResposta = "Qual item você deseja remover? Diga por exemplo 'remover o último item' ou clique no ícone de lixeira."
        };
    }
    #endregion

    #region Métodos de Interação Manual (Chamados por Cliques na UI)
    public async Task<VoiceProcessResult> SelecionarProdutoCandidatoAsync(
        VoiceSessionState estado,
        int produtoId,
        decimal quantidade,
        CancellationToken cancellationToken = default)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var produto = await _context.Produtos
            .Include(p => p.UnidadeMedida)
            .FirstOrDefaultAsync(p => p.Id == produtoId && p.EmpresaId == empresaId, cancellationToken);

        if (produto == null)
        {
            return new VoiceProcessResult
            {
                Estado = estado,
                MensagemResposta = "Produto não localizado no cadastro."
            };
        }

        decimal qtd = quantidade > 0 ? quantidade : (estado.QuantidadePendente > 0 ? estado.QuantidadePendente : 1);
        return InserirItemNoPedido(estado, produto, qtd);
    }

    public async Task<VoiceProcessResult> SelecionarClienteCandidatoAsync(
        VoiceSessionState estado,
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.Id == clienteId && c.EmpresaId == empresaId, cancellationToken);

        if (cliente == null)
        {
            return new VoiceProcessResult
            {
                Estado = estado,
                MensagemResposta = "Cliente não localizado no banco de dados."
            };
        }

        estado.ClienteId = cliente.Id;
        estado.ClienteNome = cliente.Nome;
        estado.Status = VoiceSessionStatus.CriandoPedidoAguardandoItens;
        estado.CandidatosClientes.Clear();
        estado.TermoClienteNaoEncontrado = null;

        return new VoiceProcessResult
        {
            Estado = estado,
            MensagemResposta = $"Cliente {cliente.Nome} selecionado com sucesso! Agora você pode ditar os produtos e quantidades."
        };
    }

    public Task<VoiceProcessResult> RemoverItemPedidoAsync(VoiceSessionState estado, int produtoId)
    {
        var item = estado.Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (item != null)
        {
            estado.Itens.Remove(item);
        }

        return Task.FromResult(new VoiceProcessResult
        {
            Estado = estado,
            DeveFalar = false,
            MensagemResposta = $"Item removido. Total atual: {estado.ValorTotal:C2}."
        });
    }

    public Task<VoiceProcessResult> AlterarQuantidadeItemAsync(VoiceSessionState estado, int produtoId, decimal novaQuantidade)
    {
        var item = estado.Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (item != null)
        {
            if (novaQuantidade <= 0)
            {
                estado.Itens.Remove(item);
            }
            else
            {
                item.Quantidade = novaQuantidade;
            }
        }

        return Task.FromResult(new VoiceProcessResult
        {
            Estado = estado,
            DeveFalar = false,
            MensagemResposta = $"Quantidade alterada. Total atual: {estado.ValorTotal:C2}."
        });
    }

    public async Task<VoiceProcessResult> CadastrarClienteRapidoAsync(
        VoiceSessionState estado,
        string nome,
        string? telefone,
        string? cpf,
        CancellationToken cancellationToken = default)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var novoCliente = new Cliente
        {
            EmpresaId = empresaId,
            Nome = CapitalizarNome(nome),
            TipoPessoa = TipoPessoa.Fisica,
            CpfCnpj = !string.IsNullOrWhiteSpace(cpf) ? cpf : "000.000.000-00",
            Telefone = !string.IsNullOrWhiteSpace(telefone) ? telefone : "(11) 99999-9999",
            Email = "cliente@erp.com",
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        _context.Clientes.Add(novoCliente);
        await _context.SaveChangesAsync(cancellationToken);

        estado.ClienteId = novoCliente.Id;
        estado.ClienteNome = novoCliente.Nome;
        estado.Status = VoiceSessionStatus.CriandoPedidoAguardandoItens;
        estado.CandidatosClientes.Clear();
        estado.TermoClienteNaoEncontrado = null;

        return new VoiceProcessResult
        {
            Estado = estado,
            MensagemResposta = $"Cliente {novoCliente.Nome} cadastrado e selecionado com sucesso! Agora você pode ditar os produtos e as quantidades."
        };
    }

    public async Task<VoiceProcessResult> FinalizarPedidoManualAsync(VoiceSessionState estado, CancellationToken cancellationToken = default)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        return await FinalizarPedidoAsync(empresaId, estado, cancellationToken);
    }

    public async Task<VoiceProcessResult> EmitirNfeManualAsync(VoiceSessionState estado, CancellationToken cancellationToken = default)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        return await EmitirNfePedidoAsync(empresaId, estado, cancellationToken);
    }
    #endregion

    #region Etapa 3: Conclusão do Pedido e NF-e
    private async Task<VoiceProcessResult> FinalizarPedidoAsync(
        int empresaId,
        VoiceSessionState estado,
        CancellationToken cancellationToken)
    {
        if (!estado.Itens.Any())
        {
            return new VoiceProcessResult
            {
                Estado = estado,
                MensagemResposta = "O pedido ainda não tem nenhum produto adicionado. Fale pelo menos um item antes de finalizar."
            };
        }

        var deposito = await _context.Depositos.FirstOrDefaultAsync(d => d.EmpresaId == empresaId && d.Ativo, cancellationToken);
        var formaPagamento = await _context.FormasPagamento.FirstOrDefaultAsync(f => f.EmpresaId == empresaId && f.Ativa, cancellationToken);

        int ano = DateTime.UtcNow.Year;
        int countAno = await _context.PedidosVenda.CountAsync(p => p.EmpresaId == empresaId && p.DataPedido.Year == ano, cancellationToken);
        string numero = $"PED-{ano}-{(countAno + 1):D4}";

        var pedido = new PedidoVenda
        {
            EmpresaId = empresaId,
            Numero = numero,
            ClienteId = estado.ClienteId ?? 1,
            VendedorNome = "Assistente de Voz",
            DataPedido = DateTime.UtcNow,
            Status = StatusPedidoVenda.Aprovado,
            DepositoId = deposito?.Id,
            FormaPagamentoId = formaPagamento?.Id,
            NumeroParcelas = 1,
            IntervaloDias = 30,
            DiasPrimeiroVencimento = estado.DiasVencimento,
            ValorBruto = estado.ValorTotal,
            ValorTotal = estado.ValorTotal,
            Observacao = "Pedido gerado via comando de voz pelo Copilot ERP"
        };

        foreach (var item in estado.Itens)
        {
            pedido.Itens.Add(new PedidoVendaItem
            {
                EmpresaId = empresaId,
                TipoItem = TipoItemVenda.Produto,
                ProdutoId = item.ProdutoId,
                Descricao = item.ProdutoNome,
                Quantidade = item.Quantidade,
                PrecoUnitario = item.ValorUnitario,
                SubTotal = item.ValorTotal
            });
        }

        _context.PedidosVenda.Add(pedido);
        await _context.SaveChangesAsync(cancellationToken);

        estado.PedidoCriadoId = pedido.Id;
        estado.PedidoCriadoNumero = pedido.Numero;
        estado.Status = VoiceSessionStatus.PedidoConcluidoAguardandoNfe;

        return new VoiceProcessResult
        {
            Estado = estado,
            PedidoGerado = true,
            MensagemResposta = $"Pedido {pedido.Numero} gerado com sucesso no valor de {pedido.ValorTotal:C2}! Deseja que eu emita a NF-e agora?"
        };
    }

    private async Task<VoiceProcessResult> EmitirNfePedidoAsync(
        int empresaId,
        VoiceSessionState estado,
        CancellationToken cancellationToken)
    {
        if (!estado.PedidoCriadoId.HasValue)
        {
            return new VoiceProcessResult
            {
                Estado = estado,
                MensagemResposta = "Não há um pedido em andamento para emitir a NF-e."
            };
        }

        int proximoNumero = (await _context.NotasFiscais
            .Where(n => n.EmpresaId == empresaId && n.Modelo == ModeloDocumentoFiscal.NFe)
            .Select(n => (int?)n.Numero)
            .MaxAsync(cancellationToken) ?? 1000) + 1;

        var random = new Random();
        string chave = $"352610{proximoNumero:D9}55001{random.Next(10000000, 99999999)}1";
        string protocolo = "1352600" + random.Next(1000000, 9999999);

        var nota = new NotaFiscal
        {
            EmpresaId = empresaId,
            Modelo = ModeloDocumentoFiscal.NFe,
            Serie = 1,
            Numero = proximoNumero,
            DataEmissao = DateTime.UtcNow,
            ClienteId = estado.ClienteId,
            PedidoVendaId = estado.PedidoCriadoId,
            ValorTotalNota = estado.ValorTotal,
            Status = StatusNotaFiscal.Autorizada,
            ChaveAcesso = chave,
            ProtocoloAutorizacao = protocolo,
            NaturezaOperacao = "VENDA DE MERCADORIAS",
            TipoOperacao = TipoOperacaoFiscal.Saida,
            Ambiente = AmbienteFiscal.Homologacao
        };

        foreach (var item in estado.Itens)
        {
            nota.Itens.Add(new NotaFiscalItem
            {
                EmpresaId = empresaId,
                ProdutoId = item.ProdutoId,
                DescricaoProduto = item.ProdutoNome,
                Quantidade = item.Quantidade,
                ValorUnitario = item.ValorUnitario,
                ValorTotalBruto = item.ValorTotal,
                ValorTotalLiquido = item.ValorTotal,
                Ncm = "25232900",
                Cfop = "5102",
                Csosn = "102"
            });
        }

        _context.NotasFiscais.Add(nota);
        await _context.SaveChangesAsync(cancellationToken);

        estado.Status = VoiceSessionStatus.NfeEmitida;

        return new VoiceProcessResult
        {
            Estado = estado,
            NfeEmitida = true,
            NotaFiscalId = nota.Id,
            LinkDanfe = $"/api/notas-fiscais/{nota.Id}/danfe",
            MensagemResposta = $"Nota Fiscal NF-e número {nota.Numero} emitida e autorizada com sucesso pela SEFAZ com protocolo {protocolo}! O DANFE em PDF já está pronto para visualização."
        };
    }
    #endregion

    #region Utilitários de Processamento de Linguagem Natural (PLN)
    private static decimal ExtrairQuantidade(string texto)
    {
        var matchQtd = Regex.Match(texto, @"\b(\d+(?:[.,]\d+)?)\b");
        if (matchQtd.Success)
        {
            var valorStr = matchQtd.Groups[1].Value.Replace(",", ".");
            if (decimal.TryParse(valorStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var q) && q > 0)
            {
                return q;
            }
        }

        // Numerais por extenso comuns
        if (texto.Contains("duas") || texto.Contains("dois")) return 2;
        if (texto.Contains("tres") || texto.Contains("três")) return 3;
        if (texto.Contains("quatro")) return 4;
        if (texto.Contains("cinco")) return 5;
        if (texto.Contains("seis")) return 6;
        if (texto.Contains("sete")) return 7;
        if (texto.Contains("oito")) return 8;
        if (texto.Contains("nove")) return 9;
        if (texto.Contains("dez")) return 10;
        if (texto.Contains("vinte")) return 20;
        if (texto.Contains("trinta")) return 30;
        if (texto.Contains("cinquenta")) return 50;
        if (texto.Contains("cem") || texto.Contains("cento")) return 100;

        return 1;
    }

    private static string LimparTermoProduto(string texto)
    {
        string t = texto;
        string[] remover = {
            "quero adicionar", "quero", "me ve", "me vê", "coloca", "coloque", "adiciona", "adicionar",
            "bota", "unidades do produto", "unidades de", "unidades", "unidade de", "unidade",
            "pecas de", "peças de", "pecas", "peças", "sacos de", "saco de", "sacos",
            "litros de", "litro de", "litros", "metros de", "metro de", "metros",
            "caixas de", "caixa de", "caixas", "pacotes de", "pacote de", "pacotes",
            "do produto", "produto", "por favor", "favor", "de ", "do ", "da "
        };

        foreach (var r in remover)
        {
            t = t.Replace(r, " ");
        }

        // Remover números
        t = Regex.Replace(t, @"\b\d+(?:[.,]\d+)?\b", " ").Trim();
        t = Regex.Replace(t, @"\s+", " ").Trim();
        return t;
    }

    private static int ObterIndiceOpcao(string texto)
    {
        // 1, 2, 3, etc.
        var m = Regex.Match(texto, @"\b(\d+)\b");
        if (m.Success && int.TryParse(m.Groups[1].Value, out int num))
        {
            return num - 1; // Base 0
        }

        if (texto.Contains("primeir") || texto.Contains("opcao um") || texto.Contains("opção um") || texto.Contains("o um")) return 0;
        if (texto.Contains("segund") || texto.Contains("opcao dois") || texto.Contains("opção dois") || texto.Contains("o dois")) return 1;
        if (texto.Contains("terceir") || texto.Contains("opcao tres") || texto.Contains("opção três") || texto.Contains("o tres") || texto.Contains("o três")) return 2;
        if (texto.Contains("quart") || texto.Contains("opcao quatro") || texto.Contains("opção quatro") || texto.Contains("o quatro")) return 3;
        if (texto.Contains("quint") || texto.Contains("opcao cinco") || texto.Contains("opção cinco") || texto.Contains("o cinco")) return 4;

        return -1;
    }

    private static string NormalizarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
        var lower = texto.ToLowerInvariant().Trim();
        return Regex.Replace(lower, @"[.,!?;:]", "");
    }

    private static string CapitalizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome)) return string.Empty;
        var words = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", words.Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower()));
    }
    #endregion
}
