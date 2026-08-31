using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities;

/// <summary>
/// Vínculo entre Usuário e Empresa.
/// Um usuário pode pertencer a múltiplas empresas,
/// e em cada empresa pode ter papéis/permissões diferentes.
/// </summary>
public class UsuarioEmpresa : BaseEntity
{
    public string UsuarioId { get; set; } = string.Empty;
    public int EmpresaId { get; set; }
    public bool Ativo { get; set; } = true;

    // Navegação
    public Empresa Empresa { get; set; } = null!;
}
