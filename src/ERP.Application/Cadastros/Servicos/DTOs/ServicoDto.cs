namespace ERP.Application.Cadastros.Servicos.DTOs;

public class ServicoDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int? CategoriaId { get; set; }
    public string? CategoriaNome { get; set; }
    public decimal PrecoBase { get; set; }
    public string? Unidade { get; set; }
    public string? CodigoServicoCnae { get; set; }
    public decimal? AliquotaIss { get; set; }
    public bool Ativo { get; set; }
}

public class ServicoListItemDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? CategoriaNome { get; set; }
    public decimal PrecoBase { get; set; }
    public string? Unidade { get; set; }
    public bool Ativo { get; set; }
}
