namespace ERP.Domain.Common;

/// <summary>
/// Entidade base com campos de auditoria automática.
/// CriadoEm/CriadoPor e AlteradoEm/AlteradoPor são preenchidos
/// automaticamente pelo AuditableEntityInterceptor.
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime CriadoEm { get; set; }
    public string? CriadoPor { get; set; }
    public DateTime? AlteradoEm { get; set; }
    public string? AlteradoPor { get; set; }
    public bool Ativo { get; set; } = true;
}
