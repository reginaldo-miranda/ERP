using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ERP.Application.Common.Interfaces;
using ERP.Application.Compras.Common;
using ERP.Application.Compras.DTOs;
using ERP.Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Services.Compras;

public class NfeXmlParserService : INfeXmlParserService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public NfeXmlParserService(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    {
        _context = context;
        _empresaService = empresaService;
    }

    public async Task<NfeXmlParseResultDto> ParseNfeXmlAsync(string xmlContent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
            throw new ArgumentException("O conteúdo do XML não pode ser vazio.", nameof(xmlContent));

        var result = new NfeXmlParseResultDto
        {
            XmlConteudo = xmlContent
        };

        XDocument doc;
        try
        {
            doc = XDocument.Parse(xmlContent);
        }
        catch (Exception ex)
        {
            result.PodeProsseguir = false;
            result.MensagemErroBloqueante = $"Arquivo XML inválido ou mal formatado: {ex.Message}";
            return result;
        }

        // Localiza a tag infNFe independentemente do namespace SEFAZ
        var infNFe = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "infNFe");
        if (infNFe == null)
        {
            result.PodeProsseguir = false;
            result.MensagemErroBloqueante = "Estrutura de NF-e inválida: elemento <infNFe> não encontrado no arquivo.";
            return result;
        }

        // 1. Chave de Acesso (44 dígitos)
        string chaveBruta = infNFe.Attribute("Id")?.Value ?? string.Empty;
        string chaveLimpa = Regex.Replace(chaveBruta, @"[^\d]", "");
        if (chaveLimpa.Length >= 44)
        {
            result.ChaveAcessoNfe = chaveLimpa[^44..]; // Pega os 44 dígitos finais
        }
        else
        {
            // Tenta pegar de protNFe se existir
            var chNFeTag = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "chNFe");
            if (chNFeTag != null)
                result.ChaveAcessoNfe = Regex.Replace(chNFeTag.Value, @"[^\d]", "");
        }

        // 2. Identificação da NF-e (tag ide)
        var ide = infNFe.Elements().FirstOrDefault(e => e.Name.LocalName == "ide");
        if (ide != null)
        {
            result.NumeroNotaFiscal = ide.Elements().FirstOrDefault(e => e.Name.LocalName == "nNF")?.Value ?? string.Empty;
            result.SerieNotaFiscal = ide.Elements().FirstOrDefault(e => e.Name.LocalName == "serie")?.Value ?? "1";
            result.NaturezaOperacao = ide.Elements().FirstOrDefault(e => e.Name.LocalName == "natOp")?.Value;

            string? dhEmiStr = ide.Elements().FirstOrDefault(e => e.Name.LocalName == "dhEmi")?.Value ??
                               ide.Elements().FirstOrDefault(e => e.Name.LocalName == "dEmi")?.Value;

            if (!string.IsNullOrEmpty(dhEmiStr) && DateTime.TryParse(dhEmiStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dtEmi))
            {
                result.DataEmissao = dtEmi;
            }
            else
            {
                result.DataEmissao = DateTime.Today;
            }
        }

        // 3. Emitente / Fornecedor (tag emit)
        var emit = infNFe.Elements().FirstOrDefault(e => e.Name.LocalName == "emit");
        if (emit != null)
        {
            string cnpjEmit = emit.Elements().FirstOrDefault(e => e.Name.LocalName == "CNPJ")?.Value ??
                              emit.Elements().FirstOrDefault(e => e.Name.LocalName == "CPF")?.Value ?? string.Empty;

            result.Fornecedor.Cnpj = FormatarCnpjCpf(cnpjEmit);
            result.Fornecedor.RazaoSocial = emit.Elements().FirstOrDefault(e => e.Name.LocalName == "xNome")?.Value ?? string.Empty;
            result.Fornecedor.NomeFantasia = emit.Elements().FirstOrDefault(e => e.Name.LocalName == "xFant")?.Value;
            result.Fornecedor.InscricaoEstadual = emit.Elements().FirstOrDefault(e => e.Name.LocalName == "IE")?.Value;

            var enderEmit = emit.Elements().FirstOrDefault(e => e.Name.LocalName == "enderEmit");
            if (enderEmit != null)
            {
                result.Fornecedor.Logradouro = enderEmit.Elements().FirstOrDefault(e => e.Name.LocalName == "xLgr")?.Value;
                result.Fornecedor.Numero = enderEmit.Elements().FirstOrDefault(e => e.Name.LocalName == "nro")?.Value;
                result.Fornecedor.Complemento = enderEmit.Elements().FirstOrDefault(e => e.Name.LocalName == "xCpl")?.Value;
                result.Fornecedor.Bairro = enderEmit.Elements().FirstOrDefault(e => e.Name.LocalName == "xBairro")?.Value;
                result.Fornecedor.Cidade = enderEmit.Elements().FirstOrDefault(e => e.Name.LocalName == "xMun")?.Value;
                result.Fornecedor.Uf = enderEmit.Elements().FirstOrDefault(e => e.Name.LocalName == "UF")?.Value;
                result.Fornecedor.Cep = enderEmit.Elements().FirstOrDefault(e => e.Name.LocalName == "CEP")?.Value;
                result.Fornecedor.Telefone = enderEmit.Elements().FirstOrDefault(e => e.Name.LocalName == "fone")?.Value;
            }
        }

        // 4. Destinatário (tag dest) e Validação de CNPJ com a Empresa Ativa
        var dest = infNFe.Elements().FirstOrDefault(e => e.Name.LocalName == "dest");
        string cnpjDestXml = string.Empty;
        if (dest != null)
        {
            cnpjDestXml = dest.Elements().FirstOrDefault(e => e.Name.LocalName == "CNPJ")?.Value ??
                          dest.Elements().FirstOrDefault(e => e.Name.LocalName == "CPF")?.Value ?? string.Empty;

            result.Destinatario.CnpjCpf = FormatarCnpjCpf(cnpjDestXml);
            result.Destinatario.RazaoSocial = dest.Elements().FirstOrDefault(e => e.Name.LocalName == "xNome")?.Value;
        }

        int empresaId = _empresaService.EmpresaId ?? 1;
        var empresaAtiva = await _context.Empresas.AsNoTracking().FirstOrDefaultAsync(e => e.Id == empresaId, cancellationToken);
        if (empresaAtiva != null)
        {
            result.Destinatario.CnpjEmpresaAtual = FormatarCnpjCpf(empresaAtiva.Cnpj);
            string cnpjDestSemPontos = LimparCaracteres(cnpjDestXml);
            string cnpjEmpresaSemPontos = LimparCaracteres(empresaAtiva.Cnpj);

            if (!string.IsNullOrEmpty(cnpjDestSemPontos) && !string.IsNullOrEmpty(cnpjEmpresaSemPontos))
            {
                bool bateu = cnpjDestSemPontos == cnpjEmpresaSemPontos;
                result.Destinatario.CnpjValidoParaEmpresaAtual = bateu;
                if (!bateu)
                {
                    result.PodeProsseguir = false;
                    result.MensagemErroBloqueante = $"Validação de segurança: O CNPJ do destinatário no XML ({result.Destinatario.CnpjCpf}) " +
                                                   $"diverge do CNPJ da empresa ativa no ERP ({result.Destinatario.CnpjEmpresaAtual}). " +
                                                   "A entrada só pode ser dada na empresa a quem a nota fiscal foi emitida.";
                }
            }
            else
            {
                result.Destinatario.CnpjValidoParaEmpresaAtual = true;
            }
        }

        // 5. Validação de Chave Duplicada
        if (!string.IsNullOrEmpty(result.ChaveAcessoNfe))
        {
            var entradaExistente = await _context.EntradasMercadoria
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmpresaId == empresaId && e.ChaveAcessoNfe == result.ChaveAcessoNfe, cancellationToken);

            if (entradaExistente != null)
            {
                result.ChaveJaCadastrada = true;
                result.ChaveExistenteEntradaNumero = entradaExistente.Numero;
                result.PodeProsseguir = false;
                result.MensagemErroBloqueante = $"Esta NF-e (Chave {result.ChaveAcessoNfe}) já foi importada anteriormente no recebimento '{entradaExistente.Numero}' em {entradaExistente.DataEntrada:dd/MM/yyyy}. Para evitar duplicidade de estoque e financeiro, a importação foi bloqueada.";
            }
        }

        // 6. Totais da Nota Fiscal (tag total/ICMSTot)
        var icmsTot = infNFe.Descendants().FirstOrDefault(e => e.Name.LocalName == "ICMSTot");
        if (icmsTot != null)
        {
            result.ValorProdutos = ParseDecimal(icmsTot.Elements().FirstOrDefault(e => e.Name.LocalName == "vProd")?.Value);
            result.ValorFrete = ParseDecimal(icmsTot.Elements().FirstOrDefault(e => e.Name.LocalName == "vFrete")?.Value);
            result.ValorSeguro = ParseDecimal(icmsTot.Elements().FirstOrDefault(e => e.Name.LocalName == "vSeg")?.Value);
            result.ValorDesconto = ParseDecimal(icmsTot.Elements().FirstOrDefault(e => e.Name.LocalName == "vDesc")?.Value);
            result.ValorOutrasDespesas = ParseDecimal(icmsTot.Elements().FirstOrDefault(e => e.Name.LocalName == "vOutro")?.Value);
            result.ValorTotalNota = ParseDecimal(icmsTot.Elements().FirstOrDefault(e => e.Name.LocalName == "vNF")?.Value);
        }

        // 7. Cobrança / Duplicatas (tag cobr/dup)
        var dups = infNFe.Descendants().Where(e => e.Name.LocalName == "dup");
        foreach (var dup in dups)
        {
            string nDup = dup.Elements().FirstOrDefault(e => e.Name.LocalName == "nDup")?.Value ?? string.Empty;
            string? dVencStr = dup.Elements().FirstOrDefault(e => e.Name.LocalName == "dVenc")?.Value;
            decimal vDup = ParseDecimal(dup.Elements().FirstOrDefault(e => e.Name.LocalName == "vDup")?.Value);

            DateTime vencimento = DateTime.Today.AddDays(30);
            if (!string.IsNullOrEmpty(dVencStr) && DateTime.TryParse(dVencStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dtVenc))
            {
                vencimento = dtVenc;
            }

            result.Duplicatas.Add(new NfeXmlDuplicataDto
            {
                NumeroDuplicata = nDup,
                DataVencimento = vencimento,
                Valor = vDup
            });
        }

        // 8. Busca Fornecedor Cadastrado no ERP
        string cnpjEmitenteLimpo = LimparCaracteres(result.Fornecedor.Cnpj);
        int? fornecedorId = null;
        if (!string.IsNullOrEmpty(cnpjEmitenteLimpo))
        {
            var fornecedores = await _context.Fornecedores
                .AsNoTracking()
                .Where(f => f.EmpresaId == empresaId && f.Ativo)
                .ToListAsync(cancellationToken);

            var forn = fornecedores.FirstOrDefault(f => LimparCaracteres(f.Cnpj) == cnpjEmitenteLimpo);
            if (forn != null)
            {
                fornecedorId = forn.Id;
                result.Fornecedor.JaCadastrado = true;
                result.Fornecedor.FornecedorIdExistente = forn.Id;
            }
            else
            {
                result.Fornecedor.JaCadastrado = false;
                result.MensagemAlerta = $"O fornecedor '{result.Fornecedor.RazaoSocial}' (CNPJ: {result.Fornecedor.Cnpj}) não está cadastrado. Você poderá confirmá-lo e cadastrá-lo automaticamente nesta operação.";
            }
        }

        // 9. Extração dos Itens e De-Para com o Catálogo Interno
        var deParasExistentes = new List<ERP.Domain.Core.Entities.Compras.ProdutoFornecedor>();
        if (fornecedorId.HasValue)
        {
            deParasExistentes = await _context.ProdutosFornecedores
                .AsNoTracking()
                .Include(pf => pf.Produto)
                    .ThenInclude(p => p.UnidadeMedida)
                .Where(pf => pf.EmpresaId == empresaId && pf.FornecedorId == fornecedorId.Value && pf.Ativo)
                .ToListAsync(cancellationToken);
        }

        var todosProdutos = await _context.Produtos
            .AsNoTracking()
            .Include(p => p.UnidadeMedida)
            .Where(p => p.EmpresaId == empresaId && p.Ativo)
            .ToListAsync(cancellationToken);

        var dets = infNFe.Elements().Where(e => e.Name.LocalName == "det");
        int indexItem = 1;

        foreach (var det in dets)
        {
            var prod = det.Elements().FirstOrDefault(e => e.Name.LocalName == "prod");
            if (prod == null) continue;

            string cProd = prod.Elements().FirstOrDefault(e => e.Name.LocalName == "cProd")?.Value ?? string.Empty;
            string cEAN = prod.Elements().FirstOrDefault(e => e.Name.LocalName == "cEAN")?.Value ?? string.Empty;
            string xProd = prod.Elements().FirstOrDefault(e => e.Name.LocalName == "xProd")?.Value ?? string.Empty;
            string ncm = prod.Elements().FirstOrDefault(e => e.Name.LocalName == "NCM")?.Value ?? string.Empty;
            string cest = prod.Elements().FirstOrDefault(e => e.Name.LocalName == "CEST")?.Value ?? string.Empty;
            string cfop = prod.Elements().FirstOrDefault(e => e.Name.LocalName == "CFOP")?.Value ?? string.Empty;
            string uCom = prod.Elements().FirstOrDefault(e => e.Name.LocalName == "uCom")?.Value ?? "UN";
            decimal qCom = ParseDecimal(prod.Elements().FirstOrDefault(e => e.Name.LocalName == "qCom")?.Value);
            decimal vUnCom = ParseDecimal(prod.Elements().FirstOrDefault(e => e.Name.LocalName == "vUnCom")?.Value);
            decimal vProd = ParseDecimal(prod.Elements().FirstOrDefault(e => e.Name.LocalName == "vProd")?.Value);
            decimal vDesc = ParseDecimal(prod.Elements().FirstOrDefault(e => e.Name.LocalName == "vDesc")?.Value);
            decimal vFrete = ParseDecimal(prod.Elements().FirstOrDefault(e => e.Name.LocalName == "vFrete")?.Value);
            decimal vOutro = ParseDecimal(prod.Elements().FirstOrDefault(e => e.Name.LocalName == "vOutro")?.Value);

            // Rastreabilidade / Lote (tag rastro se existir)
            var rastro = prod.Elements().FirstOrDefault(e => e.Name.LocalName == "rastro");
            string? nLote = rastro?.Elements().FirstOrDefault(e => e.Name.LocalName == "nLote")?.Value;
            DateTime? dFab = null;
            DateTime? dVal = null;
            if (rastro != null)
            {
                string? dFabStr = rastro.Elements().FirstOrDefault(e => e.Name.LocalName == "dFab")?.Value;
                string? dValStr = rastro.Elements().FirstOrDefault(e => e.Name.LocalName == "dVal")?.Value;
                if (!string.IsNullOrEmpty(dFabStr) && DateTime.TryParse(dFabStr, out var parsedFab)) dFab = parsedFab;
                if (!string.IsNullOrEmpty(dValStr) && DateTime.TryParse(dValStr, out var parsedVal)) dVal = parsedVal;
            }

            var itemDto = new NfeXmlItemDto
            {
                NumeroItem = indexItem++,
                CodigoProdutoFornecedor = cProd.Trim(),
                DescricaoProdutoFornecedor = xProd.Trim(),
                CodigoBarrasEan = (cEAN.ToUpperInvariant() == "SEM GTIN" || string.IsNullOrWhiteSpace(cEAN)) ? null : cEAN.Trim(),
                Ncm = ncm,
                Cest = cest,
                Cfop = cfop,
                UnidadeMedidaFornecedor = uCom.Trim(),
                Quantidade = qCom,
                ValorUnitario = vUnCom,
                ValorTotal = vProd,
                ValorDesconto = vDesc,
                ValorFrete = vFrete,
                ValorOutrasDespesas = vOutro,
                CodigoLote = nLote,
                DataFabricacaoLote = dFab,
                DataValidadeLote = dVal
            };

            // ALGORITMO DE CASAMENTO DE PRODUTO:
            // 1) Vínculo De-Para existente para este Fornecedor + CodigoProdutoFornecedor
            var dePara = deParasExistentes.FirstOrDefault(dp => dp.CodigoProdutoFornecedor.Equals(cProd.Trim(), StringComparison.OrdinalIgnoreCase));
            if (dePara != null && dePara.Produto != null)
            {
                itemDto.ProdutoIdSugerido = dePara.Produto.Id;
                itemDto.ProdutoCodigoSugerido = dePara.Produto.Codigo;
                itemDto.ProdutoNomeSugerido = dePara.Produto.Nome;
                itemDto.UnidadeMedidaInterna = dePara.Produto.UnidadeMedida?.Sigla ?? "UN";
                itemDto.ControlaLote = dePara.Produto.ControlaLote;
                itemDto.ControlaSerie = dePara.Produto.ControlaSerie;
                itemDto.OrigemMapeamento = OrigemMapeamentoProduto.DeParaFornecedor;
            }
            // 2) Código de barras (cEAN) igual ao CodigoBarras do produto cadastrado
            else if (!string.IsNullOrEmpty(itemDto.CodigoBarrasEan))
            {
                var porEan = todosProdutos.FirstOrDefault(p => !string.IsNullOrEmpty(p.CodigoBarras) && p.CodigoBarras.Trim() == itemDto.CodigoBarrasEan);
                if (porEan != null)
                {
                    itemDto.ProdutoIdSugerido = porEan.Id;
                    itemDto.ProdutoCodigoSugerido = porEan.Codigo;
                    itemDto.ProdutoNomeSugerido = porEan.Nome;
                    itemDto.UnidadeMedidaInterna = porEan.UnidadeMedida?.Sigla ?? "UN";
                    itemDto.ControlaLote = porEan.ControlaLote;
                    itemDto.ControlaSerie = porEan.ControlaSerie;
                    itemDto.OrigemMapeamento = OrigemMapeamentoProduto.CodigoBarrasEan;
                }
            }

            // 3) Fallback: CodigoFornecedor no cadastro do Produto
            if (itemDto.ProdutoIdSugerido == null && !string.IsNullOrEmpty(cProd))
            {
                var porCodFornecedor = todosProdutos.FirstOrDefault(p =>
                    (!string.IsNullOrEmpty(p.CodigoFornecedor) && p.CodigoFornecedor.Trim().Equals(cProd.Trim(), StringComparison.OrdinalIgnoreCase)) ||
                    p.Codigo.Trim().Equals(cProd.Trim(), StringComparison.OrdinalIgnoreCase));

                if (porCodFornecedor != null)
                {
                    itemDto.ProdutoIdSugerido = porCodFornecedor.Id;
                    itemDto.ProdutoCodigoSugerido = porCodFornecedor.Codigo;
                    itemDto.ProdutoNomeSugerido = porCodFornecedor.Nome;
                    itemDto.UnidadeMedidaInterna = porCodFornecedor.UnidadeMedida?.Sigla ?? "UN";
                    itemDto.ControlaLote = porCodFornecedor.ControlaLote;
                    itemDto.ControlaSerie = porCodFornecedor.ControlaSerie;
                    itemDto.OrigemMapeamento = OrigemMapeamentoProduto.CodigoFornecedor;
                }
            }

            result.Itens.Add(itemDto);
        }

        // 10. Sugestão de Pedidos de Compra em Aberto com itens coincidentes
        if (fornecedorId.HasValue)
        {
            var pedidosAbertos = await _context.PedidosCompra
                .AsNoTracking()
                .Include(p => p.Itens)
                .Where(p => p.EmpresaId == empresaId &&
                            p.FornecedorId == fornecedorId.Value &&
                            (p.Status == StatusPedidoCompra.Aprovado || p.Status == StatusPedidoCompra.ParcialmenteRecebido))
                .OrderByDescending(p => p.DataPedido)
                .Take(5)
                .ToListAsync(cancellationToken);

            var produtosDoXmlIds = result.Itens.Where(i => i.ProdutoIdSugerido.HasValue).Select(i => i.ProdutoIdSugerido!.Value).ToHashSet();

            foreach (var ped in pedidosAbertos)
            {
                int coincidencias = ped.Itens.Count(pi => produtosDoXmlIds.Contains(pi.ProdutoId));
                result.PedidosSugeridos.Add(new NfeXmlPedidoSugeridoDto
                {
                    PedidoId = ped.Id,
                    NumeroPedido = ped.Numero,
                    DataPedido = ped.DataPedido,
                    ValorTotal = ped.ValorTotal,
                    ItensCoincidentes = coincidencias
                });
            }
        }

        return result;
    }

    private static decimal ParseDecimal(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return 0m;
        if (decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
            return dec;
        if (decimal.TryParse(valor, NumberStyles.Any, new CultureInfo("pt-BR"), out dec))
            return dec;
        return 0m;
    }

    private static string LimparCaracteres(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        return Regex.Replace(input, @"[^\d]", "");
    }

    private static string FormatarCnpjCpf(string? doc)
    {
        string limpo = LimparCaracteres(doc);
        if (limpo.Length == 14)
            return Convert.ToUInt64(limpo).ToString(@"00\.000\.000\/0000\-00");
        if (limpo.Length == 11)
            return Convert.ToUInt64(limpo).ToString(@"000\.000\.000\-00");
        return doc ?? string.Empty;
    }
}
