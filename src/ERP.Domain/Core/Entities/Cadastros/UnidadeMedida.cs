using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities.Cadastros;

/// <summary>
/// Unidade de medida usada em produtos (ex: UN, KG, L, M, M², CX).
/// </summary>
public class UnidadeMedida : BaseEmpresaEntity, IAggregateRoot
{
    public string Sigla { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;

    // Navegação
    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
