using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities;

/// <summary>
/// Moeda do sistema (BRL, USD, EUR, etc.).
/// </summary>
public class Moeda : BaseAuditableEntity
{
    public string Codigo { get; set; } = string.Empty; // ISO 4217: BRL, USD, EUR
    public string Nome { get; set; } = string.Empty;    // Real Brasileiro, Dólar Americano
    public string Simbolo { get; set; } = string.Empty; // R$, $, €
    public int CasasDecimais { get; set; } = 2;
}
