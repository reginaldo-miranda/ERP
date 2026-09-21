using ERP.Domain.Core.Enums;

namespace ERP.Application.Financeiro.ContasReceber.DTOs;

public record ContaReceberListItemDto(
    int Id,
    int? ClienteId,
    string? ClienteNome,
    string Descricao,
    string? NumeroDocumento,
    decimal ValorOriginal,
    decimal ValorRecebido,
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

public record ContaReceberDto(
    int Id,
    int? ClienteId,
    string? ClienteNome,
    string Descricao,
    string? NumeroDocumento,
    decimal ValorOriginal,
    decimal ValorRecebido,
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
    List<BaixaContaReceberDto> Baixas
);

public record BaixaContaReceberDto(
    int Id,
    int ContaReceberId,
    int ContaBancariaId,
    string ContaBancariaDescricao,
    int FormaPagamentoId,
    string FormaPagamentoNome,
    DateTime DataBaixa,
    decimal ValorPrincipal,
    decimal ValorJuros,
    decimal ValorMulta,
    decimal ValorDesconto,
    decimal ValorTotalRecebido,
    string? Observacoes
);

public record ResumoContasReceberDto(
    decimal TotalVencido,
    int QuantidadeVencidos,
    decimal TotalAVencer,
    int QuantidadeAVencer,
    decimal TotalRecebidoNoMes,
    int QuantidadeRecebidosNoMes,
    decimal TotalAberto
);
