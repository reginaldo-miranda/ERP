using ERP.Application.Auth.DTOs;

namespace ERP.Application.Usuarios.DTOs;

public class UsuarioDetalhesDto
{
    public string Id { get; set; } = string.Empty;
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
    public List<string> Papeis { get; set; } = new();
    public List<EmpresaResumoDto> Empresas { get; set; } = new();
}
