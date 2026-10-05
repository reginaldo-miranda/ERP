using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Entities.Estoque;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Vendas;

/// <summary>
/// Pedido de venda. Estoque e financeiro só são afetados ao faturar:
/// a aprovação apenas valida o saldo disponível, sem reservar.
/// </summary>
public class PedidoVenda : BaseEmpresaEntity, IAggregateRoot
{
    /// <summary>Número sequencial por empresa/ano, ex.: PED-2026-0001.</summary>
    public string Numero { get; set; } = string.Empty;

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int? OrcamentoId { get; set; }
    public Orcamento? Orcamento { get; set; }

    public string? VendedorId { get; set; }
    public string? VendedorNome { get; set; }

    public DateTime DataPedido { get; set; } = DateTime.UtcNow;
    public StatusPedidoVenda Status { get; set; } = StatusPedidoVenda.Pendente;

    /// <summary>Depósito de onde sai o estoque dos itens do tipo produto.</summary>
    public int? DepositoId { get; set; }
    public Deposito? Deposito { get; set; }

    // Pagamento
    public int? FormaPagamentoId { get; set; }
    public FormaPagamento? FormaPagamento { get; set; }
    public int NumeroParcelas { get; set; } = 1;
    public int IntervaloDias { get; set; } = 30;
    /// <summary>Dias entre o faturamento e o vencimento da 1ª parcela (0 = à vista).</summary>
    public int DiasPrimeiroVencimento { get; set; }

    // Totais
    public decimal ValorBruto { get; set; }
    public TipoDescontoVenda DescontoTipo { get; set; } = TipoDescontoVenda.Valor;
    public decimal DescontoGeral { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }

    public string? Observacao { get; set; }

    public DateTime? DataAprovacao { get; set; }
    public DateTime? DataCancelamento { get; set; }
    public string? MotivoCancelamento { get; set; }

    public ICollection<PedidoVendaItem> Itens { get; set; } = new List<PedidoVendaItem>();
    public ICollection<Faturamento> Faturamentos { get; set; } = new List<Faturamento>();
}
