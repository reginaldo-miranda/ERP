namespace ERP.Application.Auth.DTOs;

/// <summary>
/// DTO do usuário com dados básicos e empresas.
/// </summary>
public class UsuarioDto
{
    public string Id { get; set; } = string.Empty;
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int EmpresaAtualId { get; set; }
    public List<EmpresaResumoDto> Empresas { get; set; } = new();
    public List<string> Permissoes { get; set; } = new();
    public List<string> Papeis { get; set; } = new();
}

/// <summary>
/// DTO resumido de empresa para o seletor do TopBar.
/// </summary>
public class EmpresaResumoDto
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string Cnpj { get; set; } = string.Empty;
}
