namespace ERP.Application.Cadastros.Produtos.DTOs;

public class ProdutoDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int? CategoriaId { get; set; }
    public string? CategoriaNome { get; set; }
    public int? UnidadeMedidaId { get; set; }
    public string? UnidadeMedidaSigla { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal PrecoCusto { get; set; }
    public decimal EstoqueMinimo { get; set; }
    public bool ControlaLote { get; set; }
    public bool ControlaSerie { get; set; }
    public string? Ncm { get; set; }
    public string? Cest { get; set; }
    public string? Cfop { get; set; }
    public string? CodigoBarras { get; set; }
    public bool Ativo { get; set; }
}

public class ProdutoListItemDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? CategoriaNome { get; set; }
    public string? UnidadeSigla { get; set; }
    public decimal PrecoVenda { get; set; }
    public bool Ativo { get; set; }
}
