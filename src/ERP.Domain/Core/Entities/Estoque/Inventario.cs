using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Estoque;

/// <summary>
/// Cabeçalho de um processo de inventário físico de estoque para conciliação e ajuste.
/// </summary>
public class Inventario : BaseEmpresaEntity, IAggregateRoot
{
    public int DepositoId { get; set; }
    public Deposito Deposito { get; set; } = null!;

    public int? CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public string Protocolo { get; set; } = string.Empty;
    public DateTime DataAbertura { get; set; } = DateTime.UtcNow;
    public DateTime? DataFechamento { get; set; }
    public StatusInventario Status { get; set; } = StatusInventario.Aberto;

    public string Responsavel { get; set; } = string.Empty;
    public string? Observacao { get; set; }

    // Itens inventariados
    public ICollection<InventarioItem> Itens { get; set; } = new List<InventarioItem>();
}
