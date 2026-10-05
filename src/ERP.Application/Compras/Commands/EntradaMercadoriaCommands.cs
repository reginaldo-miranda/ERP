using ERP.Application.Common.Interfaces;
using ERP.Application.Compras.DTOs;
using ERP.Domain.Core.Entities.Compras;
using ERP.Domain.Core.Entities.Estoque;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Compras.Commands;

public record RegistrarEntradaMercadoriaCommand(RegistrarEntradaMercadoriaDto Dto) : IRequest<int>;

public class RegistrarEntradaMercadoriaCommandHandler : IRequestHandler<RegistrarEntradaMercadoriaCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public RegistrarEntradaMercadoriaCommandHandler(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    {
        _context = context;
        _empresaService = empresaService;
    }

    public async Task<int> Handle(RegistrarEntradaMercadoriaCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        if (dto.Itens == null || !dto.Itens.Any())
            throw new InvalidOperationException("A entrada de mercadorias deve conter ao menos um item.");

        var empresaId = _empresaService.EmpresaId ?? 1;

        // Valida duplicidade de NF para o mesmo fornecedor
        var nfExistente = await _context.EntradasMercadoria
            .AnyAsync(e => e.EmpresaId == empresaId &&
                           e.FornecedorId == dto.FornecedorId &&
                           e.NumeroNotaFiscal == dto.NumeroNotaFiscal &&
                           e.SerieNotaFiscal == dto.SerieNotaFiscal &&
                           e.Status == StatusEntradaMercadoria.Conferida, cancellationToken);
        if (nfExistente)
            throw new InvalidOperationException($"Já existe uma entrada conferida para o fornecedor com a NF {dto.NumeroNotaFiscal} (Série: {dto.SerieNotaFiscal ?? "-"}).");

        var ano = DateTime.UtcNow.Year;
        var countAno = await _context.EntradasMercadoria
            .IgnoreQueryFilters()
            .CountAsync(e => e.EmpresaId == empresaId && e.CriadoEm.Year == ano, cancellationToken);
        var numeroFormatado = $"REC-{ano}-{(countAno + 1):D6}";

        var idParcelamento = Guid.NewGuid();

        var entrada = new EntradaMercadoria
        {
            EmpresaId = empresaId,
            Numero = numeroFormatado,
            PedidoCompraId = dto.PedidoCompraId,
            FornecedorId = dto.FornecedorId,
            DepositoId = dto.DepositoId,
            NumeroNotaFiscal = dto.NumeroNotaFiscal,
            SerieNotaFiscal = dto.SerieNotaFiscal,
            ChaveAcessoNfe = dto.ChaveAcessoNfe,
            DataEmissao = dto.DataEmissao,
            DataEntrada = dto.DataEntrada,
            Status = StatusEntradaMercadoria.Conferida,
            ConferenteNome = dto.ConferenteNome ?? "Conferente Responsável",
            ValorFrete = dto.ValorFrete,
            ValorSeguro = dto.ValorSeguro,
            ValorOutrasDespesas = dto.ValorOutrasDespesas,
            ValorDesconto = dto.ValorDesconto,
            IdParcelamento = idParcelamento,
            Observacoes = dto.Observacoes,
            CriadoEm = DateTime.UtcNow,
            Ativo = true
        };

        decimal valorProdutos = 0m;

        // Processa cada item conferido
        foreach (var itemDto in dto.Itens)
        {
            if (itemDto.Quantidade <= 0)
                throw new InvalidOperationException("A quantidade recebida deve ser maior que zero.");
            if (itemDto.CustoUnitario < 0)
                throw new InvalidOperationException("O custo unitário não pode ser negativo.");

            var custoTotalItem = itemDto.Quantidade * itemDto.CustoUnitario;
            valorProdutos += custoTotalItem;

            var entradaItem = new EntradaMercadoriaItem
            {
                EmpresaId = empresaId,
                PedidoCompraItemId = itemDto.PedidoCompraItemId,
                ProdutoId = itemDto.ProdutoId,
                Quantidade = itemDto.Quantidade,
                CustoUnitario = itemDto.CustoUnitario,
                CustoTotal = custoTotalItem,
                LoteId = itemDto.LoteId,
                CodigoLote = itemDto.CodigoLote,
                DataValidadeLote = itemDto.DataValidadeLote,
                DataFabricacaoLote = itemDto.DataFabricacaoLote,
                NumerosSerieTexto = itemDto.NumerosSerieTexto,
                Observacao = itemDto.Observacao,
                CriadoEm = DateTime.UtcNow,
                Ativo = true
            };
            entrada.Itens.Add(entradaItem);

            // 1. Atualiza item do Pedido de Compra se vinculado
            if (itemDto.PedidoCompraItemId.HasValue)
            {
                var pedidoItem = await _context.PedidoCompraItens
                    .FirstOrDefaultAsync(pi => pi.Id == itemDto.PedidoCompraItemId.Value, cancellationToken);
                if (pedidoItem != null)
                {
                    pedidoItem.QuantidadeEntregue += itemDto.Quantidade;
                    pedidoItem.AlteradoEm = DateTime.UtcNow;
                }
            }

            // 2. Atualiza Saldo de Estoque e Custo Médio Ponderado no Depósito
            var saldoEstoque = await _context.EstoqueProdutos
                .FirstOrDefaultAsync(ep => ep.EmpresaId == empresaId &&
                                           ep.ProdutoId == itemDto.ProdutoId &&
                                           ep.DepositoId == dto.DepositoId, cancellationToken);

            if (saldoEstoque == null)
            {
                saldoEstoque = new EstoqueProduto
                {
                    EmpresaId = empresaId,
                    ProdutoId = itemDto.ProdutoId,
                    DepositoId = dto.DepositoId,
                    Quantidade = itemDto.Quantidade,
                    CustoMedio = itemDto.CustoUnitario,
                    CustoUltimaCompra = itemDto.CustoUnitario,
                    CriadoEm = DateTime.UtcNow,
                    Ativo = true
                };
                _context.EstoqueProdutos.Add(saldoEstoque);
            }
            else
            {
                // Recalcula Custo Médio Ponderado
                var qtdAnterior = saldoEstoque.Quantidade;
                var custoAnterior = saldoEstoque.CustoMedio;
                var qtdNova = qtdAnterior + itemDto.Quantidade;

                if (qtdNova > 0)
                {
                    var valorTotalPonderado = (Math.Max(0, qtdAnterior) * custoAnterior) + (itemDto.Quantidade * itemDto.CustoUnitario);
                    saldoEstoque.CustoMedio = valorTotalPonderado / (Math.Max(0, qtdAnterior) + itemDto.Quantidade);
                }
                else
                {
                    saldoEstoque.CustoMedio = itemDto.CustoUnitario;
                }

                saldoEstoque.Quantidade += itemDto.Quantidade;
                saldoEstoque.CustoUltimaCompra = itemDto.CustoUnitario;
                saldoEstoque.AlteradoEm = DateTime.UtcNow;
            }

            // 3. Atualiza Preço de Custo no cadastro principal do Produto
            var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == itemDto.ProdutoId, cancellationToken);
            if (produto != null)
            {
                produto.PrecoCusto = itemDto.CustoUnitario;
                produto.AlteradoEm = DateTime.UtcNow;
            }

            // 4. Trata Lotes (Fase 4B)
            int? loteIdFinal = itemDto.LoteId;
            if (!loteIdFinal.HasValue && !string.IsNullOrWhiteSpace(itemDto.CodigoLote))
            {
                var loteExistente = await _context.Lotes
                    .FirstOrDefaultAsync(l => l.EmpresaId == empresaId &&
                                              l.ProdutoId == itemDto.ProdutoId &&
                                              l.DepositoId == dto.DepositoId &&
                                              l.CodigoLote == itemDto.CodigoLote.Trim(), cancellationToken);

                if (loteExistente != null)
                {
                    loteExistente.QuantidadeAtual += itemDto.Quantidade;
                    loteExistente.AlteradoEm = DateTime.UtcNow;
                    loteIdFinal = loteExistente.Id;
                }
                else
                {
                    var novoLote = new Lote
                    {
                        EmpresaId = empresaId,
                        ProdutoId = itemDto.ProdutoId,
                        DepositoId = dto.DepositoId,
                        CodigoLote = itemDto.CodigoLote.Trim(),
                        DataFabricacao = itemDto.DataFabricacaoLote ?? DateTime.UtcNow,
                        DataValidade = itemDto.DataValidadeLote ?? DateTime.UtcNow.AddMonths(12),
                        QuantidadeInicial = itemDto.Quantidade,
                        QuantidadeAtual = itemDto.Quantidade,
                        CustoUnitario = itemDto.CustoUnitario,
                        Observacoes = $"Entrada via NF {dto.NumeroNotaFiscal}",
                        CriadoEm = DateTime.UtcNow,
                        Ativo = true
                    };
                    _context.Lotes.Add(novoLote);
                    await _context.SaveChangesAsync(cancellationToken);
                    loteIdFinal = novoLote.Id;
                }
                entradaItem.LoteId = loteIdFinal;
            }
            else if (loteIdFinal.HasValue)
            {
                var lote = await _context.Lotes.FirstOrDefaultAsync(l => l.Id == loteIdFinal.Value, cancellationToken);
                if (lote != null)
                {
                    lote.QuantidadeAtual += itemDto.Quantidade;
                    lote.AlteradoEm = DateTime.UtcNow;
                }
            }

            // 5. Trata Números de Série (Fase 4B)
            if (!string.IsNullOrWhiteSpace(itemDto.NumerosSerieTexto))
            {
                var series = itemDto.NumerosSerieTexto
                    .Split(new[] { ',', '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s));

                foreach (var serie in series)
                {
                    var serieExistente = await _context.NumerosSerie
                        .AnyAsync(ns => ns.EmpresaId == empresaId && ns.ProdutoId == itemDto.ProdutoId && ns.Numero == serie, cancellationToken);

                    if (!serieExistente)
                    {
                        _context.NumerosSerie.Add(new NumeroSerie
                        {
                            EmpresaId = empresaId,
                            ProdutoId = itemDto.ProdutoId,
                            Numero = serie,
                            Status = StatusNumeroSerie.Disponivel,
                            LoteId = loteIdFinal,
                            CriadoEm = DateTime.UtcNow,
                            Ativo = true
                        });
                    }
                }
            }

            // 6. Registra Movimentação de Estoque
            _context.MovimentacoesEstoque.Add(new MovimentacaoEstoque
            {
                EmpresaId = empresaId,
                ProdutoId = itemDto.ProdutoId,
                DepositoOrigemId = dto.DepositoId,
                DepositoDestinoId = null,
                Tipo = TipoMovimentacaoEstoque.Entrada,
                Quantidade = itemDto.Quantidade,
                CustoUnitario = itemDto.CustoUnitario,
                CustoTotal = custoTotalItem,
                Origem = OrigemMovimentacaoEstoque.Compra,
                DocumentoOrigem = $"NF {dto.NumeroNotaFiscal} - {numeroFormatado}",
                DocumentoOrigemId = null,
                LoteId = loteIdFinal,
                DataMovimentacao = DateTime.UtcNow,
                CriadoEm = DateTime.UtcNow,
                Ativo = true
            });
        }

        entrada.ValorProdutos = valorProdutos;
        entrada.ValorTotal = (valorProdutos - dto.ValorDesconto) + dto.ValorFrete + dto.ValorSeguro + dto.ValorOutrasDespesas;

        _context.EntradasMercadoria.Add(entrada);

        // 7. Atualiza Status do Pedido de Compra se vinculado
        if (dto.PedidoCompraId.HasValue)
        {
            var pedido = await _context.PedidosCompra
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == dto.PedidoCompraId.Value, cancellationToken);

            if (pedido != null)
            {
                bool todosEntregues = pedido.Itens.All(i => i.QuantidadeEntregue >= i.Quantidade);
                pedido.Status = todosEntregues ? StatusPedidoCompra.Recebido : StatusPedidoCompra.ParcialmenteRecebido;
                pedido.AlteradoEm = DateTime.UtcNow;
            }
        }

        // 8. Gera Contas a Pagar no Financeiro com rateio exato de centavos
        if (entrada.ValorTotal > 0)
        {
            int nParcelas = dto.NumeroParcelas > 0 ? dto.NumeroParcelas : 1;
            int intervalo = dto.IntervaloDias > 0 ? dto.IntervaloDias : 30;
            int carencia = dto.DiasPrimeiroVencimento;

            decimal valorParcelaBase = Math.Floor((entrada.ValorTotal / nParcelas) * 100) / 100m;
            decimal somaBase = valorParcelaBase * nParcelas;
            decimal centavosRestantes = entrada.ValorTotal - somaBase;

            DateTime dataVencimentoBase = dto.DataEmissao.AddDays(carencia);
            if (carencia == 0) dataVencimentoBase = dto.DataEntrada;

            for (int i = 1; i <= nParcelas; i++)
            {
                decimal valorEstaParcela = (i == 1) ? valorParcelaBase + centavosRestantes : valorParcelaBase;
                DateTime vencimentoParcela = (i == 1) ? dataVencimentoBase : dataVencimentoBase.AddDays((i - 1) * intervalo);

                var contaPagar = new ContaPagar
                {
                    EmpresaId = empresaId,
                    FornecedorId = dto.FornecedorId,
                    PlanoContaId = dto.PlanoContaId,
                    CentroCustoId = dto.CentroCustoId,
                    FormaPagamentoId = dto.FormaPagamentoId,
                    NumeroDocumento = $"{dto.NumeroNotaFiscal}/{i}",
                    DataEmissao = dto.DataEmissao,
                    DataVencimento = vencimentoParcela,
                    DataCompetencia = dto.DataEmissao,
                    ValorOriginal = valorEstaParcela,
                    ValorPago = 0,
                    SaldoRestante = valorEstaParcela,
                    Status = StatusContaFinanceira.Pendente,
                    IdParcelamento = idParcelamento,
                    NumeroParcela = i,
                    TotalParcelas = nParcelas,
                    Descricao = $"Compra NF {dto.NumeroNotaFiscal} - Parc {i}/{nParcelas}",
                    CriadoEm = DateTime.UtcNow,
                    Ativo = true
                };

                _context.ContasPagar.Add(contaPagar);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return entrada.Id;
    }
}

public record CancelarEntradaMercadoriaCommand(int Id, string Motivo) : IRequest<bool>;

public class CancelarEntradaMercadoriaCommandHandler : IRequestHandler<CancelarEntradaMercadoriaCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public CancelarEntradaMercadoriaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CancelarEntradaMercadoriaCommand request, CancellationToken cancellationToken)
    {
        var entrada = await _context.EntradasMercadoria
            .Include(e => e.Itens)
            .Include(e => e.PedidoCompra)
                .ThenInclude(p => p!.Itens)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (entrada == null)
            throw new KeyNotFoundException("Entrada de mercadorias não encontrada.");

        if (entrada.Status == StatusEntradaMercadoria.Cancelada)
            throw new InvalidOperationException("Esta entrada já está cancelada.");

        // Verifica se há parcelas pagas no Financeiro
        if (entrada.IdParcelamento.HasValue)
        {
            var titulos = await _context.ContasPagar
                .Where(c => c.IdParcelamento == entrada.IdParcelamento.Value)
                .ToListAsync(cancellationToken);

            if (titulos.Any(c => c.Status == StatusContaFinanceira.Paga || c.ValorPago > 0))
            {
                throw new InvalidOperationException("Não é possível cancelar a entrada pois existem parcelas de Contas a Pagar já baixadas ou pagas. Estorne os pagamentos primeiro.");
            }

            foreach (var t in titulos)
            {
                t.Status = StatusContaFinanceira.Cancelada;
                t.AlteradoEm = DateTime.UtcNow;
            }
        }

        // Estorno no Estoque
        foreach (var item in entrada.Itens)
        {
            var saldo = await _context.EstoqueProdutos
                .FirstOrDefaultAsync(ep => ep.ProdutoId == item.ProdutoId && ep.DepositoId == entrada.DepositoId, cancellationToken);

            if (saldo != null)
            {
                saldo.Quantidade -= item.Quantidade;
                saldo.AlteradoEm = DateTime.UtcNow;
            }

            if (item.LoteId.HasValue)
            {
                var lote = await _context.Lotes.FirstOrDefaultAsync(l => l.Id == item.LoteId.Value, cancellationToken);
                if (lote != null)
                {
                    lote.QuantidadeAtual = Math.Max(0, lote.QuantidadeAtual - item.Quantidade);
                    lote.AlteradoEm = DateTime.UtcNow;
                }
            }

            _context.MovimentacoesEstoque.Add(new MovimentacaoEstoque
            {
                EmpresaId = entrada.EmpresaId,
                ProdutoId = item.ProdutoId,
                DepositoOrigemId = entrada.DepositoId,
                DepositoDestinoId = null,
                Tipo = TipoMovimentacaoEstoque.Saida,
                Quantidade = item.Quantidade,
                CustoUnitario = item.CustoUnitario,
                CustoTotal = item.CustoTotal,
                Origem = OrigemMovimentacaoEstoque.CancelamentoCompra,
                DocumentoOrigem = $"Canc. Entrada {entrada.Numero} (NF {entrada.NumeroNotaFiscal})",
                DocumentoOrigemId = entrada.Id,
                LoteId = item.LoteId,
                DataMovimentacao = DateTime.UtcNow,
                CriadoEm = DateTime.UtcNow,
                Ativo = true
            });

            // Reverte QuantidadeEntregue no Pedido de Compra se vinculado
            if (item.PedidoCompraItemId.HasValue && entrada.PedidoCompra != null)
            {
                var pItem = entrada.PedidoCompra.Itens.FirstOrDefault(pi => pi.Id == item.PedidoCompraItemId.Value);
                if (pItem != null)
                {
                    pItem.QuantidadeEntregue = Math.Max(0, pItem.QuantidadeEntregue - item.Quantidade);
                    pItem.AlteradoEm = DateTime.UtcNow;
                }
            }
        }

        if (entrada.PedidoCompra != null)
        {
            entrada.PedidoCompra.Status = StatusPedidoCompra.Aprovado;
            entrada.PedidoCompra.AlteradoEm = DateTime.UtcNow;
        }

        entrada.Status = StatusEntradaMercadoria.Cancelada;
        entrada.DataCancelamento = DateTime.UtcNow;
        entrada.MotivoCancelamento = request.Motivo;
        entrada.AlteradoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
