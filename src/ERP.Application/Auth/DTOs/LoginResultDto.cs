namespace ERP.Application.Auth.DTOs;

/// <summary>
/// DTO com resultado do login: tokens e dados do usuário.
/// </summary>
public class LoginResultDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenExpiracao { get; set; }
    public UsuarioDto Usuario { get; set; } = null!;
}
