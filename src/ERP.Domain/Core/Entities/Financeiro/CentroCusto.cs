using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities.Financeiro;

/// <summary>
/// Centros de custo para alocação gerencial de receitas e despesas.
/// </summary>
public class CentroCusto : BaseEmpresaEntity, IAggregateRoot
{
    public string Codigo { get; set; } = string.Empty; // ex: "01", "02.01"
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}
