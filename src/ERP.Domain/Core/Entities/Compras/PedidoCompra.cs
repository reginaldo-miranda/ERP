using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Entities.Estoque;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Compras;

/// <summary>
/// Pedido formal de compra enviado ao fornecedor.
/// </summary>
public class PedidoCompra : BaseEmpresaEntity, IAggregateRoot
{
    /// <summary>Número sequencial formatado, ex.: PC-2026-000001.</summary>
    public string Numero { get; set; } = string.Empty;

    public int FornecedorId { get; set; }
    public Fornecedor Fornecedor { get; set; } = null!;

    public int? SolicitacaoCompraId { get; set; }
    public SolicitacaoCompra? SolicitacaoCompra { get; set; }

    public DateTime DataPedido { get; set; } = DateTime.UtcNow;
    public DateTime? PrevisaoEntrega { get; set; }

    public StatusPedidoCompra Status { get; set; } = StatusPedidoCompra.Pendente;
    public string? CompradorNome { get; set; }

    /// <summary>Depósito padrão onde as mercadorias devem ser descarregadas.</summary>
    public int DepositoDestinoId { get; set; }
    public Deposito DepositoDestino { get; set; } = null!;

    // Condição Financeira
    public int? FormaPagamentoId { get; set; }
    public FormaPagamento? FormaPagamento { get; set; }
    public int NumeroParcelas { get; set; } = 1;
    public int IntervaloDias { get; set; } = 30;
    public int DiasPrimeiroVencimento { get; set; }

    // Valores
    public decimal ValorBruto { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorSeguro { get; set; }
    public decimal ValorOutrasDespesas { get; set; }
    public decimal ValorTotal { get; set; }

    public string? Observacoes { get; set; }

    public DateTime? DataAprovacao { get; set; }
    public DateTime? DataCancelamento { get; set; }
    public string? MotivoCancelamento { get; set; }

    public ICollection<PedidoCompraItem> Itens { get; set; } = new List<PedidoCompraItem>();
    public ICollection<EntradaMercadoria> Entradas { get; set; } = new List<EntradaMercadoria>();
}
