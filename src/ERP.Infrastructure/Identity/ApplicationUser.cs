using Microsoft.AspNetCore.Identity;

namespace ERP.Infrastructure.Identity;

/// <summary>
/// Usuário do sistema baseado no ASP.NET Identity.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
