using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities;

/// <summary>
/// Registro de auditoria automática.
/// Cada alteração em qualquer entidade gera um registro nesta tabela.
/// </summary>
public class AuditLog : BaseEntity
{
    public int? EmpresaId { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string TableName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // INSERT, UPDATE, DELETE

    /// <summary>
    /// Valores anteriores em formato JSON (null para INSERT).
    /// Armazenado como JSONB no PostgreSQL.
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// Valores novos em formato JSON (null para DELETE).
    /// Armazenado como JSONB no PostgreSQL.
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// Lista de colunas alteradas em formato JSON (null para INSERT/DELETE).
    /// </summary>
    public string? ChangedColumns { get; set; }

    public string? IpAddress { get; set; }
}
