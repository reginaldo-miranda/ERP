using ERP.Domain.Core.Enums;

namespace ERP.Application.Compras.DTOs;

#region Solicitações de Compra DTOs

public record SolicitacaoCompraListItemDto(
    int Id,
    string Numero,
    string SolicitanteNome,
    string? Departamento,
    DateTime DataSolicitacao,
    DateTime? DataNecessidade,
    StatusSolicitacaoCompra Status,
    PrioridadeSolicitacaoCompra Prioridade,
    int TotalItens,
    string? Justificativa,
    string? AprovadorNome,
    DateTime? DataAprovacao
);

public record SolicitacaoCompraItemDto(
    int? Id,
    int ProdutoId,
    string ProdutoNome,
    string ProdutoCodigo,
    string UnidadeMedida,
    decimal Quantidade,
    decimal QuantidadeAtendida,
    string? Observacao
);

public record SolicitacaoCompraDetalheDto(
    int Id,
    string Numero,
    string SolicitanteId,
    string SolicitanteNome,
    string? Departamento,
    DateTime DataSolicitacao,
    DateTime? DataNecessidade,
    StatusSolicitacaoCompra Status,
    PrioridadeSolicitacaoCompra Prioridade,
    string? Justificativa,
    string? Observacoes,
    string? AprovadorNome,
    DateTime? DataAprovacao,
    string? MotivoRejeicao,
    List<SolicitacaoCompraItemDto> Itens
);

public record CriarSolicitacaoCompraDto(
    string? Departamento,
    DateTime? DataNecessidade,
    PrioridadeSolicitacaoCompra Prioridade,
    string? Justificativa,
    string? Observacoes,
    List<CriarSolicitacaoCompraItemDto> Itens
);

public record CriarSolicitacaoCompraItemDto(
    int ProdutoId,
    decimal Quantidade,
    string? Observacao
);

#endregion

#region Pedidos de Compra DTOs

public record PedidoCompraListItemDto(
    int Id,
    string Numero,
    int FornecedorId,
    string FornecedorNome,
    string? FornecedorCnpj,
    int? SolicitacaoCompraId,
    string? SolicitacaoNumero,
    DateTime DataPedido,
    DateTime? PrevisaoEntrega,
    StatusPedidoCompra Status,
    int DepositoDestinoId,
    string DepositoDestinoNome,
    int TotalItens,
    decimal ValorTotal,
    int NumeroParcelas,
    string? CompradorNome
);

public record PedidoCompraItemDto(
    int? Id,
    int ProdutoId,
    string ProdutoNome,
    string ProdutoCodigo,
    string UnidadeMedida,
    decimal Quantidade,
    decimal QuantidadeEntregue,
    decimal PrecoUnitario,
    decimal ValorDesconto,
    decimal SubTotal,
    string? Observacao
);

public record PedidoCompraDetalheDto(
    int Id,
    string Numero,
    int FornecedorId,
    string FornecedorNome,
    string? FornecedorCnpj,
    int? SolicitacaoCompraId,
    string? SolicitacaoNumero,
    DateTime DataPedido,
    DateTime? PrevisaoEntrega,
    StatusPedidoCompra Status,
    string? CompradorNome,
    int DepositoDestinoId,
    string DepositoDestinoNome,
    int? FormaPagamentoId,
    string? FormaPagamentoNome,
    int NumeroParcelas,
    int IntervaloDias,
    int DiasPrimeiroVencimento,
    decimal ValorBruto,
    decimal ValorDesconto,
    decimal ValorFrete,
    decimal ValorSeguro,
    decimal ValorOutrasDespesas,
    decimal ValorTotal,
    string? Observacoes,
    DateTime? DataAprovacao,
    DateTime? DataCancelamento,
    string? MotivoCancelamento,
    List<PedidoCompraItemDto> Itens
);

public record CriarPedidoCompraDto(
    int FornecedorId,
    int? SolicitacaoCompraId,
    DateTime DataPedido,
    DateTime? PrevisaoEntrega,
    string? CompradorNome,
    int DepositoDestinoId,
    int? FormaPagamentoId,
    int NumeroParcelas,
    int IntervaloDias,
    int DiasPrimeiroVencimento,
    decimal ValorFrete,
    decimal ValorSeguro,
    decimal ValorOutrasDespesas,
    string? Observacoes,
    List<CriarPedidoCompraItemDto> Itens
);

public record CriarPedidoCompraItemDto(
    int ProdutoId,
    decimal Quantidade,
    decimal PrecoUnitario,
    decimal ValorDesconto,
    string? Observacao
);

#endregion

#region Entrada de Mercadorias DTOs

public record EntradaMercadoriaListItemDto(
    int Id,
    string Numero,
    int? PedidoCompraId,
    string? PedidoCompraNumero,
    int FornecedorId,
    string FornecedorNome,
    int DepositoId,
    string DepositoNome,
    string NumeroNotaFiscal,
    string? SerieNotaFiscal,
    DateTime DataEmissao,
    DateTime DataEntrada,
    StatusEntradaMercadoria Status,
    int TotalItens,
    decimal ValorTotal,
    string? ConferenteNome
);

public record EntradaMercadoriaItemDto(
    int? Id,
    int? PedidoCompraItemId,
    int ProdutoId,
    string ProdutoNome,
    string ProdutoCodigo,
    string UnidadeMedida,
    bool ControlaLote,
    bool ControlaSerie,
    decimal QuantidadePedida,
    decimal Quantidade,
    decimal CustoUnitario,
    decimal CustoTotal,
    int? LoteId,
    string? CodigoLote,
    DateTime? DataValidadeLote,
    DateTime? DataFabricacaoLote,
    string? NumerosSerieTexto,
    string? Observacao
);

public record EntradaMercadoriaDetalheDto(
    int Id,
    string Numero,
    int? PedidoCompraId,
    string? PedidoCompraNumero,
    int FornecedorId,
    string FornecedorNome,
    string? FornecedorCnpj,
    int DepositoId,
    string DepositoNome,
    string NumeroNotaFiscal,
    string? SerieNotaFiscal,
    string? ChaveAcessoNfe,
    DateTime DataEmissao,
    DateTime DataEntrada,
    StatusEntradaMercadoria Status,
    string? ConferenteNome,
    decimal ValorProdutos,
    decimal ValorFrete,
    decimal ValorSeguro,
    decimal ValorOutrasDespesas,
    decimal ValorDesconto,
    decimal ValorTotal,
    Guid? IdParcelamento,
    string? Observacoes,
    List<EntradaMercadoriaItemDto> Itens
);

public record RegistrarEntradaMercadoriaDto(
    int? PedidoCompraId,
    int FornecedorId,
    int DepositoId,
    string NumeroNotaFiscal,
    string? SerieNotaFiscal,
    string? ChaveAcessoNfe,
    DateTime DataEmissao,
    DateTime DataEntrada,
    string? ConferenteNome,
    decimal ValorFrete,
    decimal ValorSeguro,
    decimal ValorOutrasDespesas,
    decimal ValorDesconto,
    string? Observacoes,
    // Condições financeiras para gerar Contas a Pagar
    int? FormaPagamentoId,
    int? PlanoContaId,
    int? CentroCustoId,
    int NumeroParcelas,
    int IntervaloDias,
    int DiasPrimeiroVencimento,
    List<RegistrarEntradaMercadoriaItemDto> Itens
);

public record RegistrarEntradaMercadoriaItemDto(
    int? PedidoCompraItemId,
    int ProdutoId,
    decimal Quantidade,
    decimal CustoUnitario,
    int? LoteId,
    string? CodigoLote,
    DateTime? DataValidadeLote,
    DateTime? DataFabricacaoLote,
    string? NumerosSerieTexto,
    string? Observacao
);

#endregion

#region Relatórios & Histórico DTOs

public record HistoricoComprasItemDto(
    int EntradaId,
    string EntradaNumero,
    string NotaFiscal,
    DateTime DataEntrada,
    string FornecedorNome,
    string ProdutoCodigo,
    string ProdutoNome,
    string UnidadeMedida,
    decimal Quantidade,
    decimal CustoUnitario,
    decimal CustoTotal,
    string DepositoNome
);

public record ComparativoPrecoFornecedorDto(
    int FornecedorId,
    string FornecedorNome,
    DateTime UltimaCompraData,
    string UltimaNotaFiscal,
    decimal MenorPreco,
    decimal MaiorPreco,
    decimal UltimoPreco,
    decimal QuantidadeTotalComprada
);

public record ResumoComprasDto(
    int TotalPedidos,
    int PedidosPendentes,
    int PedidosRecebidos,
    decimal ValorTotalComprado,
    int TotalEntradas,
    decimal TicketMedioPedido
);

#endregion
