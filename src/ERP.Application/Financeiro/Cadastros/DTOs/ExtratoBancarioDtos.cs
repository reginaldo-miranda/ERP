using ERP.Domain.Core.Enums;

namespace ERP.Application.Financeiro.Cadastros.DTOs;

/// <summary>
/// DTO principal do extrato bancário retornado pela query.
/// </summary>
public record ExtratoBancarioDto(
    int ContaBancariaId,
    string ContaBancariaDescricao,
    DateTime DataInicio,
    DateTime DataFim,
    decimal SaldoInicial,
    decimal SaldoFinal,
    decimal TotalEntradas,
    decimal TotalSaidas,
    List<ExtratoBancarioItemDto> Movimentacoes
);

/// <summary>
/// Item individual de movimentação no extrato, com saldo acumulado.
/// </summary>
public record ExtratoBancarioItemDto(
    int Id,
    DateTime DataMovimentacao,
    string Descricao,
    TipoOperacaoFinanceira Tipo,
    decimal Valor,
    decimal SaldoAcumulado,
    string? PlanoConta,
    string? CentroCusto,
    string? DocumentoReferencia,
    string? Observacoes
);
