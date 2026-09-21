using ERP.Domain.Core.Enums;

namespace ERP.Application.Financeiro.ContasPagar.DTOs;

public record ContaPagarListItemDto(
    int Id,
    int? FornecedorId,
    string? FornecedorNome,
    string Descricao,
    string? NumeroDocumento,
    decimal ValorOriginal,
    decimal ValorPago,
    decimal SaldoRestante,
    DateTime DataEmissao,
    DateTime DataVencimento,
    StatusContaFinanceira Status,
    int NumeroParcela,
    int TotalParcelas,
    string? FormaPagamentoNome,
    string? ContaBancariaDescricao,
    string? PlanoContaDescricao,
    string? CentroCustoDescricao,
    bool Vencida
);

public record ContaPagarDto(
    int Id,
    int? FornecedorId,
    string? FornecedorNome,
    string Descricao,
    string? NumeroDocumento,
    decimal ValorOriginal,
    decimal ValorPago,
    decimal SaldoRestante,
    DateTime DataEmissao,
    DateTime DataVencimento,
    DateTime? DataCompetencia,
    StatusContaFinanceira Status,
    int? FormaPagamentoId,
    string? FormaPagamentoNome,
    int? ContaBancariaId,
    string? ContaBancariaDescricao,
    int? PlanoContaId,
    string? PlanoContaDescricao,
    int? CentroCustoId,
    string? CentroCustoDescricao,
    int NumeroParcela,
    int TotalParcelas,
    Guid? IdParcelamento,
    string? Observacoes,
    List<BaixaContaPagarDto> Baixas
);

public record BaixaContaPagarDto(
    int Id,
    int ContaPagarId,
    int ContaBancariaId,
    string ContaBancariaDescricao,
    int FormaPagamentoId,
    string FormaPagamentoNome,
    DateTime DataBaixa,
    decimal ValorPrincipal,
    decimal ValorJuros,
    decimal ValorMulta,
    decimal ValorDesconto,
    decimal ValorTotalPago,
    string? Observacoes
);

public record ResumoContasPagarDto(
    decimal TotalVencido,
    int QuantidadeVencidos,
    decimal TotalAVencer,
    int QuantidadeAVencer,
    decimal TotalPagoNoMes,
    int QuantidadePagosNoMes,
    decimal TotalAberto
);
