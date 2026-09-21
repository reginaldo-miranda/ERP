using ERP.Application.Common.Interfaces;
using ERP.Application.Financeiro.FluxoCaixa.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ERP.Application.Financeiro.FluxoCaixa.Queries;

public record GetFluxoCaixaQuery(
    DateTime DataInicio,
    DateTime DataFim,
    int? ContaBancariaId = null,
    TipoAgrupamentoFluxoCaixa Agrupamento = TipoAgrupamentoFluxoCaixa.Diario,
    bool IncluirProjetado = true
) : IRequest<FluxoCaixaResultDto>;

public class GetFluxoCaixaQueryHandler : IRequestHandler<GetFluxoCaixaQuery, FluxoCaixaResultDto>
{
    private readonly IApplicationDbContext _context;

    public GetFluxoCaixaQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FluxoCaixaResultDto> Handle(GetFluxoCaixaQuery request, CancellationToken cancellationToken)
    {
        var dataInicio = request.DataInicio.ToUniversalTime().Date;
        var dataFim = request.DataFim.ToUniversalTime().Date.AddDays(1).AddTicks(-1);

        // 1. Saldo Inicial — soma SaldoInicial das contas + movimentações anteriores ao período
        var queryContas = _context.ContasBancarias.AsNoTracking().Where(c => c.Ativa);
        if (request.ContaBancariaId.HasValue)
            queryContas = queryContas.Where(c => c.Id == request.ContaBancariaId.Value);

        var contasIds = await queryContas.Select(c => c.Id).ToListAsync(cancellationToken);
        var saldoInicialContas = await queryContas.SumAsync(c => c.SaldoInicial, cancellationToken);

        var queryMovAntes = _context.MovimentacoesFinanceiras.AsNoTracking()
            .Where(m => m.DataMovimentacao < dataInicio && contasIds.Contains(m.ContaBancariaId));

        var movAntes = await queryMovAntes.ToListAsync(cancellationToken);
        decimal saldoAnterior = movAntes.Sum(m =>
            m.Tipo == TipoOperacaoFinanceira.Entrada ? m.Valor :
            m.Tipo == TipoOperacaoFinanceira.Saida ? -m.Valor : 0);

        decimal saldoInicial = saldoInicialContas + saldoAnterior;

        // 2. Movimentações Realizadas no período
        var queryMov = _context.MovimentacoesFinanceiras.AsNoTracking()
            .Include(m => m.PlanoConta)
            .Where(m => m.DataMovimentacao >= dataInicio && m.DataMovimentacao <= dataFim);

        if (request.ContaBancariaId.HasValue)
            queryMov = queryMov.Where(m => m.ContaBancariaId == request.ContaBancariaId.Value);
        else if (contasIds.Any())
            queryMov = queryMov.Where(m => contasIds.Contains(m.ContaBancariaId));

        var movimentacoes = await queryMov.ToListAsync(cancellationToken);

        var itensRaw = new List<(DateTime Data, string? Categoria, string Descricao, decimal Entrada, decimal Saida, string Tipo, string? Origem)>();

        foreach (var mov in movimentacoes)
        {
            itensRaw.Add((
                Data: mov.DataMovimentacao,
                Categoria: mov.PlanoConta?.Descricao,
                Descricao: mov.Descricao,
                Entrada: mov.Tipo == TipoOperacaoFinanceira.Entrada ? mov.Valor : 0,
                Saida: mov.Tipo == TipoOperacaoFinanceira.Saida ? mov.Valor : 0,
                Tipo: "Realizado",
                Origem: "Movimentação"
            ));
        }

        // 3. Projetado
        if (request.IncluirProjetado)
        {
            // Contas a Receber pendentes — entradas projetadas
            var queryCR = _context.ContasReceber.AsNoTracking()
                .Include(c => c.PlanoConta)
                .Include(c => c.Cliente)
                .Where(c => c.Status != StatusContaFinanceira.Paga &&
                            c.Status != StatusContaFinanceira.Cancelada &&
                            c.DataVencimento >= dataInicio && c.DataVencimento <= dataFim);

            if (request.ContaBancariaId.HasValue)
                queryCR = queryCR.Where(c => c.ContaBancariaId == null || c.ContaBancariaId == request.ContaBancariaId.Value);

            var contasReceber = await queryCR.ToListAsync(cancellationToken);
            foreach (var cr in contasReceber)
            {
                itensRaw.Add((
                    Data: cr.DataVencimento,
                    Categoria: cr.PlanoConta?.Descricao,
                    Descricao: cr.Cliente != null ? cr.Cliente.Nome : cr.Descricao,
                    Entrada: cr.SaldoRestante,
                    Saida: 0,
                    Tipo: "Projetado",
                    Origem: "Conta a Receber"
                ));
            }

            // Contas a Pagar pendentes — saídas projetadas
            var queryCP = _context.ContasPagar.AsNoTracking()
                .Include(c => c.PlanoConta)
                .Include(c => c.Fornecedor)
                .Where(c => c.Status != StatusContaFinanceira.Paga &&
                            c.Status != StatusContaFinanceira.Cancelada &&
                            c.DataVencimento >= dataInicio && c.DataVencimento <= dataFim);

            if (request.ContaBancariaId.HasValue)
                queryCP = queryCP.Where(c => c.ContaBancariaId == null || c.ContaBancariaId == request.ContaBancariaId.Value);

            var contasPagar = await queryCP.ToListAsync(cancellationToken);
            foreach (var cp in contasPagar)
            {
                itensRaw.Add((
                    Data: cp.DataVencimento,
                    Categoria: cp.PlanoConta?.Descricao,
                    Descricao: cp.Fornecedor != null ? cp.Fornecedor.RazaoSocial : cp.Descricao,
                    Entrada: 0,
                    Saida: cp.SaldoRestante,
                    Tipo: "Projetado",
                    Origem: "Conta a Pagar"
                ));
            }
        }

        // 4. Ordenar e calcular saldo acumulado
        var itensOrdenados = itensRaw.OrderBy(i => i.Data).ThenBy(i => i.Tipo).ToList();
        var itens = new List<FluxoCaixaItemDto>();
        decimal saldoAcumulado = saldoInicial;

        foreach (var raw in itensOrdenados)
        {
            saldoAcumulado += raw.Entrada - raw.Saida;
            itens.Add(new FluxoCaixaItemDto(
                Data: raw.Data,
                Categoria: raw.Categoria,
                Descricao: raw.Descricao,
                Entrada: raw.Entrada,
                Saida: raw.Saida,
                SaldoAcumulado: saldoAcumulado,
                Tipo: raw.Tipo,
                Origem: raw.Origem
            ));
        }

        // 5. Agrupar por período
        var culture = new CultureInfo("pt-BR");
        var grupos = new List<(string Label, DateTime DataInicio, DateTime DataFim, List<FluxoCaixaItemDto> Itens)>();

        foreach (var item in itens)
        {
            string label;
            DateTime grpInicio, grpFim;

            switch (request.Agrupamento)
            {
                case TipoAgrupamentoFluxoCaixa.Semanal:
                    int diff = (7 + (item.Data.DayOfWeek - DayOfWeek.Monday)) % 7;
                    grpInicio = item.Data.AddDays(-diff).Date;
                    grpFim = grpInicio.AddDays(6).Date;
                    label = $"{grpInicio:dd/MM} - {grpFim:dd/MM}";
                    break;
                case TipoAgrupamentoFluxoCaixa.Mensal:
                    grpInicio = new DateTime(item.Data.Year, item.Data.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    grpFim = grpInicio.AddMonths(1).AddDays(-1).Date;
                    var mesLabel = item.Data.ToString("MMM/yyyy", culture);
                    label = char.ToUpper(mesLabel[0]) + mesLabel[1..];
                    break;
                default: // Diário
                    grpInicio = item.Data.Date;
                    grpFim = item.Data.Date;
                    label = item.Data.ToString("dd/MM");
                    break;
            }

            var grupo = grupos.LastOrDefault();
            if (grupo.Label == label)
            {
                grupo.Itens.Add(item);
            }
            else
            {
                grupos.Add((label, grpInicio, grpFim, new List<FluxoCaixaItemDto> { item }));
            }
        }

        var periodos = grupos.Select(g => new FluxoCaixaPeriodoDto(
            DataInicio: g.DataInicio,
            DataFim: g.DataFim,
            Label: g.Label,
            TotalEntradas: g.Itens.Sum(i => i.Entrada),
            TotalSaidas: g.Itens.Sum(i => i.Saida),
            Saldo: g.Itens.Sum(i => i.Entrada - i.Saida),
            SaldoAcumulado: g.Itens.Last().SaldoAcumulado,
            Itens: g.Itens
        )).ToList();

        // 6. Gráfico
        var grafico = new FluxoCaixaGraficoDto(
            Labels: periodos.Select(p => p.Label).ToList(),
            SerieEntradas: periodos.Select(p => (double)p.TotalEntradas).ToList(),
            SerieSaidas: periodos.Select(p => (double)p.TotalSaidas).ToList(),
            SerieSaldoAcumulado: periodos.Select(p => (double)p.SaldoAcumulado).ToList()
        );

        // 7. Resumo
        decimal totalEntradas = itens.Sum(i => i.Entrada);
        decimal totalSaidas = itens.Sum(i => i.Saida);
        decimal saldoPeriodo = totalEntradas - totalSaidas;
        decimal saldoProjetado = saldoInicial + saldoPeriodo;

        var resumo = new FluxoCaixaResumoDto(
            TotalEntradas: totalEntradas,
            TotalSaidas: totalSaidas,
            SaldoPeriodo: saldoPeriodo,
            SaldoProjetado: saldoProjetado,
            SaldoInicialPeriodo: saldoInicial
        );

        return new FluxoCaixaResultDto(
            Resumo: resumo,
            Periodos: periodos,
            Grafico: grafico
        );
    }
}
