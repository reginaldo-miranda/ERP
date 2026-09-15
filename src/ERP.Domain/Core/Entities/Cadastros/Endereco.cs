using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities.Cadastros;

/// <summary>
/// Endereço associado a um Cliente ou Fornecedor.
/// Um cliente/fornecedor pode ter múltiplos endereços.
/// </summary>
public class Endereco : BaseAuditableEntity
{
    public string Logradouro { get; set; } = string.Empty;
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public int? CodigoIbge { get; set; }
    public bool Principal { get; set; } = false;

    // FK para Cliente ou Fornecedor (apenas um será preenchido)
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int? FornecedorId { get; set; }
    public Fornecedor? Fornecedor { get; set; }
}
