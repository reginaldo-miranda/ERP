using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;

namespace ERP.Domain.Core.Entities.Compras;

/// <summary>
/// Item do pedido de compra.
/// </summary>
public class PedidoCompraItem : BaseEmpresaEntity
{
    public int PedidoCompraId { get; set; }
    public PedidoCompra PedidoCompra { get; set; } = null!;

    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public decimal Quantidade { get; set; }
    public decimal QuantidadeEntregue { get; set; }

    public decimal PrecoUnitario { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal SubTotal { get; set; }

    public string? Observacao { get; set; }
}
