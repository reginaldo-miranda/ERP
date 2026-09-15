using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Cadastros;

/// <summary>
/// Categoria hierárquica para produtos e serviços.
/// Suporta múltiplos níveis via auto-referência (CategoriaPaiId).
/// </summary>
public class Categoria : BaseEmpresaEntity, IAggregateRoot
{
    public string Nome { get; set; } = string.Empty;
    public TipoCategoria Tipo { get; set; }
    public int Nivel { get; set; } = 1;

    // Auto-referência para hierarquia
    public int? CategoriaPaiId { get; set; }
    public Categoria? CategoriaPai { get; set; }
    public ICollection<Categoria> Filhos { get; set; } = new List<Categoria>();

    // Navegação reversa
    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
    public ICollection<Servico> Servicos { get; set; } = new List<Servico>();
}
