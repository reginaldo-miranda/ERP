using ERP.Domain.Core.Enums;

namespace ERP.Application.Financeiro.Cadastros.DTOs;

public record BancoDto(
    int Id,
    string Codigo,
    string Nome,
    string? NomeReduzido,
    bool Ativo
);

public record ContaBancariaDto(
    int Id,
    string Descricao,
    TipoContaBancaria Tipo,
    int? BancoId,
    string? BancoNome,
    string? Agencia,
    string? AgenciaDigito,
    string? Conta,
    string? ContaDigito,
    decimal SaldoInicial,
    decimal SaldoAtual,
    bool Ativa,
    string? Observacoes
);

public record FormaPagamentoDto(
    int Id,
    string Nome,
    TipoFormaPagamento Tipo,
    int DiasCompensacao,
    decimal TaxaPercentual,
    bool Ativa
);

public record PlanoContaDto(
    int Id,
    string Codigo,
    string Descricao,
    TipoPlanoConta Tipo,
    bool Sintetica,
    int? PlanoContaPaiId,
    string? PlanoContaPaiDescricao,
    bool Ativo
);

public record CentroCustoDto(
    int Id,
    string Codigo,
    string Descricao,
    bool Ativo
);
