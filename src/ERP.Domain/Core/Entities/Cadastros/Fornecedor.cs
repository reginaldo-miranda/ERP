using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities.Cadastros;

/// <summary>
/// Cadastro de fornecedores (sempre PJ).
/// </summary>
public class Fornecedor : BaseEmpresaEntity, IAggregateRoot
{
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string? InscricaoEstadual { get; set; }
    public string? InscricaoMunicipal { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Celular { get; set; }
    public string? Contato { get; set; }
    public string? Site { get; set; }
    public string? Observacoes { get; set; }

    // Navegação
    public ICollection<Endereco> Enderecos { get; set; } = new List<Endereco>();
}
