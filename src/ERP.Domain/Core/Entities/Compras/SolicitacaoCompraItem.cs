using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;

namespace ERP.Domain.Core.Entities.Compras;

/// <summary>
/// Item requisitado na solicitação de compra.
/// </summary>
public class SolicitacaoCompraItem : BaseEmpresaEntity
{
    public int SolicitacaoCompraId { get; set; }
    public SolicitacaoCompra SolicitacaoCompra { get; set; } = null!;

    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public decimal Quantidade { get; set; }
    public decimal QuantidadeAtendida { get; set; }

    public string? Observacao { get; set; }
}
