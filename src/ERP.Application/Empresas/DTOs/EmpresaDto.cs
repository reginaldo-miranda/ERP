namespace ERP.Application.Empresas.DTOs;

/// <summary>
/// DTO completo de Empresa para CRUD.
/// </summary>
public class EmpresaDto
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string? InscricaoEstadual { get; set; }
    public string? InscricaoMunicipal { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Uf { get; set; }
    public string? Cep { get; set; }
    public string? RegimeTributario { get; set; }
    public string? MetodoCusteio { get; set; }
    public string? LogoUrl { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}
