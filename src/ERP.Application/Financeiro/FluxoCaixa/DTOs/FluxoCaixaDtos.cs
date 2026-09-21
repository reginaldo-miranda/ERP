using System;
using System.Collections.Generic;

namespace ERP.Application.Financeiro.FluxoCaixa.DTOs;

public record FluxoCaixaResumoDto(
    decimal TotalEntradas,
    decimal TotalSaidas,
    decimal SaldoPeriodo,
    decimal SaldoProjetado,
    decimal SaldoInicialPeriodo
);

public record FluxoCaixaItemDto(
    DateTime Data,
    string? Categoria,
    string Descricao,
    decimal Entrada,
    decimal Saida,
    decimal SaldoAcumulado,
    string Tipo,
    string? Origem
);

public record FluxoCaixaPeriodoDto(
    DateTime DataInicio,
    DateTime DataFim,
    string Label,
    decimal TotalEntradas,
    decimal TotalSaidas,
    decimal Saldo,
    decimal SaldoAcumulado,
    List<FluxoCaixaItemDto> Itens
);

public record FluxoCaixaGraficoDto(
    List<string> Labels,
    List<double> SerieEntradas,
    List<double> SerieSaidas,
    List<double> SerieSaldoAcumulado
);

public record FluxoCaixaResultDto(
    FluxoCaixaResumoDto Resumo,
    List<FluxoCaixaPeriodoDto> Periodos,
    FluxoCaixaGraficoDto Grafico
);
