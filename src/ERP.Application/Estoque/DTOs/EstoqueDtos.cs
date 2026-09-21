using ERP.Domain.Core.Enums;

namespace ERP.Application.Estoque.DTOs;

public record DepositoDto(
    int Id,
    string Nome,
    string? Codigo,
    string? Endereco,
    string? Responsavel,
    bool Ativo,
    bool Padrao
);

public record CriarDepositoDto(
    string Nome,
    string? Codigo,
    string? Endereco,
    string? Responsavel,
    bool Padrao
);

public record PosicaoEstoqueDto(
    int Id,
    int ProdutoId,
    string ProdutoCodigo,
    string ProdutoNome,
    string? UnidadeMedida,
    int DepositoId,
    string DepositoNome,
    decimal Quantidade,
    decimal CustoMedio,
    decimal CustoUltimaCompra,
    decimal EstoqueMinimo,
    decimal EstoqueMaximo,
    bool AbaixoEstoqueMinimo,
    decimal ValorTotalEstoque
);

public record MovimentacaoEstoqueDto(
    int Id,
    int ProdutoId,
    string ProdutoNome,
    string ProdutoCodigo,
    int DepositoOrigemId,
    string DepositoOrigemNome,
    int? DepositoDestinoId,
    string? DepositoDestinoNome,
    TipoMovimentacaoEstoque Tipo,
    OrigemMovimentacaoEstoque Origem,
    decimal Quantidade,
    decimal CustoUnitario,
    decimal CustoTotal,
    DateTime DataMovimentacao,
    string? DocumentoOrigem,
    string? Observacao
);

public record RegistrarMovimentacaoDto(
    int ProdutoId,
    int DepositoId,
    TipoMovimentacaoEstoque Tipo,
    decimal Quantidade,
    decimal CustoUnitario,
    string? DocumentoOrigem,
    string? Observacao
);
