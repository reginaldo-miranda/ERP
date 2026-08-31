namespace ERP.Domain.Common;

/// <summary>
/// Entidade base para entidades que pertencem a uma empresa específica.
/// O EmpresaId é usado para filtro global automático de multi-tenancy.
/// </summary>
public abstract class BaseEmpresaEntity : BaseAuditableEntity
{
    public int EmpresaId { get; set; }
    public Core.Entities.Empresa Empresa { get; set; } = null!;
}
