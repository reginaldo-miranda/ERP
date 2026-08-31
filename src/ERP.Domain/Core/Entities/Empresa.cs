using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities;

/// <summary>
/// Entidade principal de Empresa.
/// Toda entidade multi-empresa referencia uma Empresa via EmpresaId.
/// </summary>
public class Empresa : BaseAuditableEntity, IAggregateRoot
{
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string? InscricaoEstadual { get; set; }
    public string? InscricaoMunicipal { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }

    // Endereço
    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Uf { get; set; }
    public string? Cep { get; set; }
    public int? CodigoIbgeMunicipio { get; set; }

    // Configurações
    public string? RegimeTributario { get; set; }
    public string? MetodoCusteio { get; set; } // CustoMedio, PEPS
    public string? LogoUrl { get; set; }

    // Navegação
    public ICollection<UsuarioEmpresa> UsuarioEmpresas { get; set; } = new List<UsuarioEmpresa>();
}
