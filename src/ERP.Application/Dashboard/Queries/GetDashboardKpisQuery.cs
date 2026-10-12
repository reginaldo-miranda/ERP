using ERP.Application.Common.Interfaces;
using ERP.Application.Dashboard.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ERP.Application.Dashboard.Queries;

public record GetDashboardKpisQuery : IRequest<DashboardResumoCompletoDto>;

public class GetDashboardKpisQueryHandler : IRequestHandler<GetDashboardKpisQuery, DashboardResumoCompletoDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public GetDashboardKpisQueryHandler(
        IApplicationDbContext context,
        ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<DashboardResumoCompletoDto> Handle(GetDashboardKpisQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var hoje = DateTime.UtcNow.Date;
        var amanha = hoje.AddDays(1);
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var proximoMes = inicioMes.AddMonths(1);
        var mesAnterior = inicioMes.AddMonths(-1);

        var resultado = new DashboardResumoCompletoDto();

        // 1. Vendas
        var pedidosEmpresa = _context.PedidosVenda.AsNoTracking()
            .Where(p => p.EmpresaId == empresaId && p.Status != StatusPedidoVenda.Cancelado);

        var vendasHoje = await pedidosEmpresa
            .Where(p => p.DataPedido >= hoje && p.DataPedido < amanha)
            .SumAsync(p => (decimal?)p.ValorTotal, cancellationToken) ?? 0;

        var vendasMes = await pedidosEmpresa
            .Where(p => p.DataPedido >= inicioMes && p.DataPedido < proximoMes)
            .SumAsync(p => (decimal?)p.ValorTotal, cancellationToken) ?? 0;

        var vendasMesAnterior = await pedidosEmpresa
            .Where(p => p.DataPedido >= mesAnterior && p.DataPedido < inicioMes)
            .SumAsync(p => (decimal?)p.ValorTotal, cancellationToken) ?? 0;

        double variacaoVendas = 0;
        if (vendasMesAnterior > 0)
        {
            variacaoVendas = (double)((vendasMes - vendasMesAnterior) / vendasMesAnterior * 100);
        }
        else if (vendasMes > 0)
        {
            variacaoVendas = 100.0;
        }

        // 2. Financeiro — Contas a Receber
        var receberAbertas = _context.ContasReceber.AsNoTracking()
            .Where(c => c.EmpresaId == empresaId &&
                        c.Status != StatusContaFinanceira.Paga &&
                        c.Status != StatusContaFinanceira.Cancelada);

        var receberHoje = await receberAbertas
            .Where(c => c.DataVencimento >= hoje && c.DataVencimento < amanha)
            .SumAsync(c => (decimal?)c.SaldoRestante, cancellationToken) ?? 0;

        var receberVencidas = await receberAbertas
            .Where(c => c.DataVencimento < hoje)
            .SumAsync(c => (decimal?)c.SaldoRestante, cancellationToken) ?? 0;

        var qtdReceberVencidas = await receberAbertas
            .CountAsync(c => c.DataVencimento < hoje, cancellationToken);

        // 3. Financeiro — Contas a Pagar
        var pagarAbertas = _context.ContasPagar.AsNoTracking()
            .Where(c => c.EmpresaId == empresaId &&
                        c.Status != StatusContaFinanceira.Paga &&
                        c.Status != StatusContaFinanceira.Cancelada);

        var pagarHoje = await pagarAbertas
            .Where(c => c.DataVencimento >= hoje && c.DataVencimento < amanha)
            .SumAsync(c => (decimal?)c.SaldoRestante, cancellationToken) ?? 0;

        var pagarVencidas = await pagarAbertas
            .Where(c => c.DataVencimento < hoje)
            .SumAsync(c => (decimal?)c.SaldoRestante, cancellationToken) ?? 0;

        var qtdPagarVencidas = await pagarAbertas
            .CountAsync(c => c.DataVencimento < hoje, cancellationToken);

        // 4. Estoque Crítico
        var produtosEmpresa = _context.Produtos.AsNoTracking()
            .Where(p => p.EmpresaId == empresaId && p.Ativo);

        var totalProdutos = await produtosEmpresa.CountAsync(cancellationToken);

        // Saldo atual de estoque por produto consolidado
        var estoqueSaldos = await _context.EstoqueProdutos.AsNoTracking()
            .Where(e => e.EmpresaId == empresaId)
            .GroupBy(e => e.ProdutoId)
            .Select(g => new { ProdutoId = g.Key, Saldo = g.Sum(e => e.Quantidade) })
            .ToListAsync(cancellationToken);

        var saldosMap = estoqueSaldos.ToDictionary(s => s.ProdutoId, s => s.Saldo);

        var produtosComMinimo = await produtosEmpresa
            .Where(p => p.EstoqueMinimo > 0)
            .Select(p => new { p.Id, p.EstoqueMinimo })
            .ToListAsync(cancellationToken);

        int produtosCriticos = produtosComMinimo.Count(p =>
        {
            saldosMap.TryGetValue(p.Id, out var saldo);
            return saldo <= p.EstoqueMinimo;
        });

        // 5. Fiscal
        var notasFiscaisMes = _context.NotasFiscais.AsNoTracking()
            .Where(n => n.EmpresaId == empresaId &&
                        n.DataEmissao >= inicioMes && n.DataEmissao < proximoMes &&
                        n.Status == StatusNotaFiscal.Autorizada);

        var totalNotasMes = await notasFiscaisMes.CountAsync(cancellationToken);
        var faturamentoFiscalMes = await notasFiscaisMes.SumAsync(n => (decimal?)n.ValorTotal, cancellationToken) ?? 0;

        resultado.Kpis = new DashboardKpisDto
        {
            VendasHoje = vendasHoje,
            VendasMes = vendasMes,
            VendasMesAnterior = vendasMesAnterior,
            PercentualVariacaoVendas = Math.Round(variacaoVendas, 1),
            ContasReceberHoje = receberHoje,
            ContasReceberVencidas = receberVencidas,
            QuantidadeReceberVencidas = qtdReceberVencidas,
            ContasPagarHoje = pagarHoje,
            ContasPagarVencidas = pagarVencidas,
            QuantidadePagarVencidas = qtdPagarVencidas,
            ProdutosEstoqueCritico = produtosCriticos,
            TotalProdutosCadastrados = totalProdutos,
            TotalNotasFiscaisMes = totalNotasMes,
            FaturamentoFiscalMes = faturamentoFiscalMes
        };

        // 6. Gráfico de Evolução (Últimos 6 meses)
        var culturaPtBr = new CultureInfo("pt-BR");
        var listaMeses = new List<string>();
        var serieVendas = new List<double>();
        var serieCompras = new List<double>();

        for (int i = 5; i >= 0; i--)
        {
            var mesRef = inicioMes.AddMonths(-i);
            var proximoMesRef = mesRef.AddMonths(1);
            var rotulo = culturaPtBr.DateTimeFormat.GetAbbreviatedMonthName(mesRef.Month).ToUpper() + "/" + mesRef.ToString("yy");
            listaMeses.Add(rotulo);

            var totalVendasMes = await _context.PedidosVenda.AsNoTracking()
                .Where(p => p.EmpresaId == empresaId &&
                            p.Status != StatusPedidoVenda.Cancelado &&
                            p.DataPedido >= mesRef && p.DataPedido < proximoMesRef)
                .SumAsync(p => (decimal?)p.ValorTotal, cancellationToken) ?? 0;

            var totalComprasMes = await _context.EntradasMercadoria.AsNoTracking()
                .Where(e => e.EmpresaId == empresaId &&
                            e.Status != StatusEntradaMercadoria.Cancelada &&
                            e.DataEntrada >= mesRef && e.DataEntrada < proximoMesRef)
                .SumAsync(e => (decimal?)e.ValorTotal, cancellationToken) ?? 0;

            serieVendas.Add((double)totalVendasMes);
            serieCompras.Add((double)totalComprasMes);
        }

        resultado.Evolucao = new DashboardGraficoEvolucaoDto
        {
            Meses = listaMeses.ToArray(),
            SerieVendas = serieVendas.ToArray(),
            SerieCompras = serieCompras.ToArray()
        };

        // 7. Gráfico Faturamento por Categoria (Últimos 90 dias)
        var data90DiasAtras = hoje.AddDays(-90);
        var itensVendidos = await _context.PedidoVendaItens.AsNoTracking()
            .Where(i => i.PedidoVenda.EmpresaId == empresaId &&
                        i.PedidoVenda.Status != StatusPedidoVenda.Cancelado &&
                        i.PedidoVenda.DataPedido >= data90DiasAtras)
            .Select(i => new
            {
                Categoria = i.Produto.Categoria != null ? i.Produto.Categoria.Nome : "Geral",
                Valor = i.ValorTotal
            })
            .ToListAsync(cancellationToken);

        var porCategoria = itensVendidos
            .GroupBy(i => i.Categoria)
            .OrderByDescending(g => g.Sum(x => x.Valor))
            .Take(5)
            .ToList();

        if (porCategoria.Any())
        {
            resultado.FaturamentoPorCategoria = new DashboardGraficoCategoriaDto
            {
                Categorias = porCategoria.Select(c => c.Key).ToArray(),
                Valores = porCategoria.Select(c => (double)c.Sum(x => x.Valor)).ToArray()
            };
        }
        else
        {
            resultado.FaturamentoPorCategoria = new DashboardGraficoCategoriaDto
            {
                Categorias = new[] { "Produtos Diversos" },
                Valores = new double[] { 1 }
            };
        }

        // 8. Pedidos Recentes
        var pedidosRecentes = await _context.PedidosVenda.AsNoTracking()
            .Include(p => p.Cliente)
            .Where(p => p.EmpresaId == empresaId)
            .OrderByDescending(p => p.DataPedido)
            .Take(6)
            .Select(p => new DashboardPedidoRecenteDto
            {
                Id = p.Id,
                Numero = p.Numero,
                Data = p.DataPedido,
                ClienteNome = p.Cliente.Nome,
                ValorTotal = p.ValorTotal,
                Status = p.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        resultado.PedidosRecentes = pedidosRecentes;

        // 9. Alertas do Sistema
        if (qtdPagarVencidas > 0)
        {
            resultado.Alertas.Add(new DashboardAlertaDto
            {
                Titulo = $"{qtdPagarVencidas} Conta(s) a Pagar Vencida(s)",
                Mensagem = $"Totalizando {pagarVencidas:C2} pendentes de pagamento imediato.",
                Tipo = "error",
                Link = "/financeiro/contas-pagar",
                Icone = "MoneyOff"
            });
        }

        if (qtdReceberVencidas > 0)
        {
            resultado.Alertas.Add(new DashboardAlertaDto
            {
                Titulo = $"{qtdReceberVencidas} Conta(s) a Receber Vencida(s)",
                Mensagem = $"Totalizando {receberVencidas:C2} aguardando cobrança.",
                Tipo = "warning",
                Link = "/financeiro/contas-receber",
                Icone = "Warning"
            });
        }

        if (produtosCriticos > 0)
        {
            resultado.Alertas.Add(new DashboardAlertaDto
            {
                Titulo = $"{produtosCriticos} Item(ns) com Estoque Crítico",
                Mensagem = "Produtos atingiram ou estão abaixo do estoque de segurança.",
                Tipo = "warning",
                Link = "/estoque/posicao",
                Icone = "Inventory"
            });
        }

        return resultado;
    }
}
