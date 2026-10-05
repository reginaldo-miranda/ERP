using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Vendas;

/// <summary>
/// Faturamento total e manual de um pedido aprovado (1 pedido = 1 faturamento ativo).
/// Ao faturar são geradas a baixa de estoque e as Contas a Receber.
/// </summary>
public class Faturamento : BaseEmpresaEntity, IAggregateRoot
{
    /// <summary>Número sequencial por empresa/ano, ex.: FAT-2026-0001.</summary>
    public string Numero { get; set; } = string.Empty;

    public int PedidoVendaId { get; set; }
    public PedidoVenda PedidoVenda { get; set; } = null!;

    public DateTime DataFaturamento { get; set; } = DateTime.UtcNow;
    public StatusFaturamento Status { get; set; } = StatusFaturamento.Emitido;

    public decimal ValorTotal { get; set; }
    /// <summary>Soma de Quantidade x CustoUnitario dos itens de produto (CMV).</summary>
    public decimal CustoTotal { get; set; }

    /// <summary>Agrupador das Contas a Receber geradas (permite localizar e cancelar as parcelas).</summary>
    public Guid IdParcelamento { get; set; }

    /// <summary>Reservado para o módulo Fiscal (NF-e/NFC-e).</summary>
    public int? NotaFiscalId { get; set; }

    public DateTime? DataCancelamento { get; set; }
}
