using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;

namespace ERP.Domain.Core.Entities.Estoque;

/// <summary>
/// Item individual conferido em um inventário físico de estoque.
/// </summary>
public class InventarioItem : BaseEntity
{
    public int InventarioId { get; set; }
    public Inventario Inventario { get; set; } = null!;

    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public int? LoteId { get; set; }
    public Lote? Lote { get; set; }

    public decimal QuantidadeSistema { get; set; }
    public decimal QuantidadeContada { get; set; }
    public decimal Diferenca => QuantidadeContada - QuantidadeSistema;

    public decimal CustoUnitario { get; set; }
    public decimal ValorDivergencia => Math.Round(Diferenca * CustoUnitario, 2);

    public bool AjusteAplicado { get; set; } = false;
    public string? Observacao { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public string? CriadoPor { get; set; }
    public DateTime? AlteradoEm { get; set; }
    public string? AlteradoPor { get; set; }
}
