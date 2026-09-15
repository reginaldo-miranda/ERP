using ERP.Domain.Core.Enums;

namespace ERP.Application.Cadastros.Categorias.DTOs;

public class CategoriaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoCategoria Tipo { get; set; }
    public int Nivel { get; set; }
    public int? CategoriaPaiId { get; set; }
    public string? CategoriaPaiNome { get; set; }
    public bool Ativo { get; set; }
    public List<CategoriaDto> Filhos { get; set; } = new();
}

public class CategoriaListItemDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string NomeCompleto { get; set; } = string.Empty; // "Pai > Filho"
    public TipoCategoria Tipo { get; set; }
    public int Nivel { get; set; }
    public bool Ativo { get; set; }
}
