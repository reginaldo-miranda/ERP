using ERP.Domain.Core.Enums;

namespace ERP.Application.Vendas.DTOs;

// ==========================================
// DTOs de Itens (Orçamentos e Pedidos)
// ==========================================

public record ItemVendaInputDto(
    TipoItemVenda TipoItem,
    int? ProdutoId,
    int? ServicoId,
    string? Descricao,
    decimal Quantidade,
    decimal PrecoUnitario,
    TipoDescontoVenda DescontoTipo,
    decimal Desconto
);

public record ItemVendaDto(
    int Id,
    TipoItemVenda TipoItem,
    int? ProdutoId,
    string? ProdutoNome,
    string? ProdutoCodigo,
    int? ServicoId,
    string? ServicoNome,
    string? ServicoCodigo,
    string Descricao,
    decimal Quantidade,
    decimal PrecoUnitario,
    TipoDescontoVenda DescontoTipo,
    decimal Desconto,
    decimal ValorDesconto,
    decimal SubTotal,
    decimal CustoUnitario
);

// ==========================================
// DTOs de Orçamento
// ==========================================

public record CriarOrcamentoDto(
    int ClienteId,
    string? VendedorId,
    string? VendedorNome,
    DateTime DataOrcamento,
    DateTime DataValidade,
    TipoDescontoVenda DescontoTipo,
    decimal DescontoGeral,
    string? Observacao,
    List<ItemVendaInputDto> Itens
);

public record AtualizarOrcamentoDto(
    int ClienteId,
    string? VendedorId,
    string? VendedorNome,
    DateTime DataOrcamento,
    DateTime DataValidade,
    TipoDescontoVenda DescontoTipo,
    decimal DescontoGeral,
    string? Observacao,
    List<ItemVendaInputDto> Itens
);

public record OrcamentoListItemDto(
    int Id,
    string Numero,
    int ClienteId,
    string ClienteNome,
    string? ClienteCpfCnpj,
    string? VendedorNome,
    DateTime DataOrcamento,
    DateTime DataValidade,
    StatusOrcamento Status,
    string StatusCalculado,
    decimal ValorBruto,
    decimal ValorDesconto,
    decimal ValorTotal,
    int? PedidoVendaId,
    string? PedidoVendaNumero,
    int TotalItens
);

public record OrcamentoDetalheDto(
    int Id,
    string Numero,
    int ClienteId,
    string ClienteNome,
    string? ClienteCpfCnpj,
    string? ClienteEmail,
    string? ClienteTelefone,
    string? VendedorId,
    string? VendedorNome,
    DateTime DataOrcamento,
    DateTime DataValidade,
    StatusOrcamento Status,
    string StatusCalculado,
    decimal ValorBruto,
    TipoDescontoVenda DescontoTipo,
    decimal DescontoGeral,
    decimal ValorDesconto,
    decimal ValorTotal,
    string? Observacao,
    int? PedidoVendaId,
    string? PedidoVendaNumero,
    DateTime CriadoEm,
    List<ItemVendaDto> Itens
);

// ==========================================
// DTOs de Pedido de Venda
// ==========================================

public record CriarPedidoVendaDto(
    int ClienteId,
    int? OrcamentoId,
    string? VendedorId,
    string? VendedorNome,
    DateTime DataPedido,
    int? DepositoId,
    int? FormaPagamentoId,
    int NumeroParcelas,
    int IntervaloDias,
    int DiasPrimeiroVencimento,
    TipoDescontoVenda DescontoTipo,
    decimal DescontoGeral,
    string? Observacao,
    List<ItemVendaInputDto> Itens
);

public record AtualizarPedidoVendaDto(
    int ClienteId,
    string? VendedorId,
    string? VendedorNome,
    DateTime DataPedido,
    int? DepositoId,
    int? FormaPagamentoId,
    int NumeroParcelas,
    int IntervaloDias,
    int DiasPrimeiroVencimento,
    TipoDescontoVenda DescontoTipo,
    decimal DescontoGeral,
    string? Observacao,
    List<ItemVendaInputDto> Itens
);

public record PedidoVendaListItemDto(
    int Id,
    string Numero,
    int ClienteId,
    string ClienteNome,
    string? ClienteCpfCnpj,
    string? VendedorNome,
    DateTime DataPedido,
    StatusPedidoVenda Status,
    string? DepositoNome,
    string? FormaPagamentoNome,
    int NumeroParcelas,
    decimal ValorBruto,
    decimal ValorDesconto,
    decimal ValorTotal,
    DateTime? DataAprovacao,
    int TotalItens,
    string? FaturamentoNumero
);

public record PedidoVendaDetalheDto(
    int Id,
    string Numero,
    int ClienteId,
    string ClienteNome,
    string? ClienteCpfCnpj,
    string? ClienteEmail,
    string? ClienteTelefone,
    int? OrcamentoId,
    string? OrcamentoNumero,
    string? VendedorId,
    string? VendedorNome,
    DateTime DataPedido,
    StatusPedidoVenda Status,
    int? DepositoId,
    string? DepositoNome,
    int? FormaPagamentoId,
    string? FormaPagamentoNome,
    int NumeroParcelas,
    int IntervaloDias,
    int DiasPrimeiroVencimento,
    decimal ValorBruto,
    TipoDescontoVenda DescontoTipo,
    decimal DescontoGeral,
    decimal ValorDesconto,
    decimal ValorTotal,
    string? Observacao,
    DateTime? DataAprovacao,
    DateTime? DataCancelamento,
    string? MotivoCancelamento,
    DateTime CriadoEm,
    List<ItemVendaDto> Itens,
    List<FaturamentoResumoDto> Faturamentos
);

// ==========================================
// DTOs de Faturamento
// ==========================================

public record FaturarPedidoDto(
    int PedidoVendaId,
    int? DepositoId,
    int? FormaPagamentoId,
    int? NumeroParcelas,
    int? IntervaloDias,
    int? DiasPrimeiroVencimento,
    string? Observacao
);

public record FaturamentoResumoDto(
    int Id,
    string Numero,
    DateTime DataFaturamento,
    StatusFaturamento Status,
    decimal ValorTotal,
    decimal CustoTotal,
    Guid IdParcelamento,
    int? NotaFiscalId,
    DateTime? DataCancelamento
);

// ==========================================
// DTOs de Histórico e Relatórios de Vendas
// ==========================================

public record HistoricoVendasItemDto(
    int PedidoId,
    string PedidoNumero,
    DateTime DataVenda,
    int ClienteId,
    string ClienteNome,
    string? VendedorNome,
    StatusPedidoVenda Status,
    string? FaturamentoNumero,
    DateTime? DataFaturamento,
    decimal ValorTotal,
    decimal CustoTotal,
    decimal LucroBruto,
    decimal MargemPercentual,
    string FormasPagamento
);

public record ResumoVendasDto(
    decimal TotalVendido,
    decimal CustoTotalVendido,
    decimal LucroBruto,
    decimal MargemGeralPercentual,
    int TotalPedidos,
    int PedidosFaturados,
    int PedidosPendentes,
    int PedidosCancelados,
    decimal TicketMedio
);
