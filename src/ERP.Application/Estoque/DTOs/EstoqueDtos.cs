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
    string? Observacao,
    int? LoteId = null,
    string? NovoCodigoLote = null,
    DateTime? NovaDataValidade = null,
    DateTime? NovaDataFabricacao = null,
    int? NumeroSerieId = null,
    string? NovoNumeroSerie = null
);

public record TransferirEstoqueDto(
    int ProdutoId,
    int DepositoOrigemId,
    int DepositoDestinoId,
    decimal Quantidade,
    int? LoteId = null,
    int? NumeroSerieId = null,
    string? DocumentoOrigem = null,
    string? Observacao = null
);

public record LoteDto(
    int Id,
    int ProdutoId,
    string ProdutoNome,
    string ProdutoCodigo,
    int DepositoId,
    string DepositoNome,
    string CodigoLote,
    DateTime? DataFabricacao,
    DateTime DataValidade,
    decimal QuantidadeInicial,
    decimal QuantidadeAtual,
    decimal CustoUnitario,
    bool Ativo,
    bool Vencido,
    int DiasParaVencer,
    string? Observacoes
);

public record CriarLoteDto(
    int ProdutoId,
    int DepositoId,
    string CodigoLote,
    DateTime? DataFabricacao,
    DateTime DataValidade,
    decimal Quantidade,
    decimal CustoUnitario,
    string? Observacoes
);

public record NumeroSerieDto(
    int Id,
    int ProdutoId,
    string ProdutoNome,
    string ProdutoCodigo,
    int DepositoId,
    string DepositoNome,
    int? LoteId,
    string? CodigoLote,
    string Numero,
    StatusNumeroSerie Status,
    DateTime DataEntrada,
    DateTime? DataSaida,
    string? Observacoes
);

public record CadastrarNumeroSerieDto(
    int ProdutoId,
    int DepositoId,
    int? LoteId,
    string Numero,
    string? Observacoes
);

public record InventarioDto(
    int Id,
    int DepositoId,
    string DepositoNome,
    int? CategoriaId,
    string? CategoriaNome,
    string Protocolo,
    DateTime DataAbertura,
    DateTime? DataFechamento,
    StatusInventario Status,
    string Responsavel,
    string? Observacao,
    int TotalItens,
    decimal TotalDivergenciaFinanceira
);

public record InventarioItemDto(
    int Id,
    int InventarioId,
    int ProdutoId,
    string ProdutoCodigo,
    string ProdutoNome,
    string? UnidadeMedida,
    int? LoteId,
    string? CodigoLote,
    decimal QuantidadeSistema,
    decimal QuantidadeContada,
    decimal Diferenca,
    decimal CustoUnitario,
    decimal ValorDivergencia,
    bool AjusteAplicado,
    string? Observacao
);

public record IniciarInventarioDto(
    int DepositoId,
    int? CategoriaId,
    string Responsavel,
    string? Observacao
);

public record AtualizarContagemItemDto(
    int ItemId,
    decimal QuantidadeContada,
    string? Observacao
);

public record FinalizarInventarioDto(
    int InventarioId,
    string? Justificativa
);

