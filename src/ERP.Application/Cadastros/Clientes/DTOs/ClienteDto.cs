using ERP.Domain.Core.Enums;

namespace ERP.Application.Cadastros.Clientes.DTOs;

public class ClienteDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoPessoa TipoPessoa { get; set; }
    public string CpfCnpj { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Celular { get; set; }
    public string? NomeFantasia { get; set; }
    public string? InscricaoEstadual { get; set; }
    public decimal? LimiteCredito { get; set; }
    public string? Observacoes { get; set; }
    public bool Ativo { get; set; }
    public List<EnderecoDto> Enderecos { get; set; } = new();
}

public class ClienteListItemDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoPessoa TipoPessoa { get; set; }
    public string CpfCnpj { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public bool Ativo { get; set; }
}

public class EnderecoDto
{
    public int Id { get; set; }
    public string Logradouro { get; set; } = string.Empty;
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public bool Principal { get; set; }
}

public class CreateEnderecoRequest
{
    public string Logradouro { get; set; } = string.Empty;
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public bool Principal { get; set; }
}
