using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Vendas;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Comissoes;

/// <summary>
/// Registro de comissão calculada automaticamente sobre um Pedido de Venda faturado.
/// </summary>
public class ComissaoVenda : BaseEmpresaEntity, IAggregateRoot
{
    public int VendedorId { get; set; }
    public Vendedor Vendedor { get; set; } = null!;

    public int PedidoVendaId { get; set; }
    public PedidoVenda PedidoVenda { get; set; } = null!;

    public int? FaturamentoId { get; set; }
    public Faturamento? Faturamento { get; set; }

    public int? RegraComissaoId { get; set; }
    public RegraComissao? RegraComissao { get; set; }

    public DateTime DataVenda { get; set; } = DateTime.UtcNow;
    public decimal ValorBaseVenda { get; set; }
    public decimal PercentualAplicado { get; set; }
    public decimal ValorComissao { get; set; }

    public StatusComissao Status { get; set; } = StatusComissao.Pendente;
    public DateTime? DataAprovacao { get; set; }
    public DateTime? DataPagamento { get; set; }
    public string? Observacoes { get; set; }
}
