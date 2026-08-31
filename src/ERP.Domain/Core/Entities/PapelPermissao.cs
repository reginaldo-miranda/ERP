using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities;

/// <summary>
/// Vínculo entre Papel (Role) e Permissão.
/// Define quais permissões cada papel/perfil possui.
/// </summary>
public class PapelPermissao : BaseEntity
{
    public string PapelId { get; set; } = string.Empty; // RoleId do ASP.NET Identity
    public int PermissaoId { get; set; }

    // Navegação
    public Permissao Permissao { get; set; } = null!;
}
