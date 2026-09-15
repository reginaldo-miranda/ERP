namespace ERP.Application.Cadastros.Fornecedores.DTOs;

public class FornecedorDto
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string? InscricaoEstadual { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Celular { get; set; }
    public string? Contato { get; set; }
    public string? Site { get; set; }
    public string? Observacoes { get; set; }
    public bool Ativo { get; set; }
    public List<EnderecoFornDto> Enderecos { get; set; } = new();
}

public class FornecedorListItemDto
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public bool Ativo { get; set; }
}

public class EnderecoFornDto
{
    public int Id { get; set; }
    public string Logradouro { get; set; } = string.Empty;
    public string? Numero { get; set; }
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public bool Principal { get; set; }
}
