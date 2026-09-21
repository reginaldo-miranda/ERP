using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities.Financeiro;

/// <summary>
/// Catálogo geral de instituições financeiras (FEBRABAN).
/// </summary>
public class Banco : BaseAuditableEntity
{
    public string Codigo { get; set; } = string.Empty; // ex: "001", "237", "341"
    public string Nome { get; set; } = string.Empty;
    public string? NomeReduzido { get; set; }
    public bool Ativo { get; set; } = true;

    // Navegação
    public ICollection<ContaBancaria> ContasBancarias { get; set; } = new List<ContaBancaria>();
}
