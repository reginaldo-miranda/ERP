using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;

namespace ERP.Domain.Core.Entities.Compras;

/// <summary>
/// Mapeamento De-Para entre o código do produto no fornecedor (vindo no XML da NF-e)
/// e o produto correspondente no catálogo interno da empresa.
/// </summary>
public class ProdutoFornecedor : BaseEmpresaEntity, IAggregateRoot
{
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public int FornecedorId { get; set; }
    public Fornecedor Fornecedor { get; set; } = null!;

    /// <summary>Código do produto conforme informado pelo fornecedor na tag &lt;cProd&gt; do XML.</summary>
    public string CodigoProdutoFornecedor { get; set; } = string.Empty;

    /// <summary>Descrição do produto conforme informado pelo fornecedor na tag &lt;xProd&gt; do XML.</summary>
    public string? DescricaoProdutoFornecedor { get; set; }

    /// <summary>Unidade de medida no fornecedor, ex: CX, FD, UN, KG.</summary>
    public string? UnidadeMedidaFornecedor { get; set; }

    /// <summary>Fator de conversão para a unidade de estoque interna (padrão 1).</summary>
    public decimal FatorConversao { get; set; } = 1.0m;
}
