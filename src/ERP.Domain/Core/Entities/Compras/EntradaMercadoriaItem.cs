using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Entities.Estoque;

namespace ERP.Domain.Core.Entities.Compras;

/// <summary>
/// Item conferido e recebido na Entrada de Mercadorias.
/// </summary>
public class EntradaMercadoriaItem : BaseEmpresaEntity
{
    public int EntradaMercadoriaId { get; set; }
    public EntradaMercadoria EntradaMercadoria { get; set; } = null!;

    public int? PedidoCompraItemId { get; set; }
    public PedidoCompraItem? PedidoCompraItem { get; set; }

    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public decimal Quantidade { get; set; }
    public decimal CustoUnitario { get; set; }
    public decimal CustoTotal { get; set; }

    // Vínculo com Lotes (Fase 4B)
    public int? LoteId { get; set; }
    public Lote? Lote { get; set; }
    public string? CodigoLote { get; set; }
    public DateTime? DataValidadeLote { get; set; }
    public DateTime? DataFabricacaoLote { get; set; }

    // Números de Série informados na conferência (Fase 4B)
    public string? NumerosSerieTexto { get; set; }

    public string? Observacao { get; set; }
}
