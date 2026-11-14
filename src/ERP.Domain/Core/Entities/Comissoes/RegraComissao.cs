using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Comissoes;

/// <summary>
/// Regra de cálculo de comissão. Pode ser específica de um vendedor ou geral (VendedorId = null).
/// </summary>
public class RegraComissao : BaseEmpresaEntity
{
    public int? VendedorId { get; set; }
    public Vendedor? Vendedor { get; set; }

    public string Nome { get; set; } = string.Empty;
    public TipoRegraComissao Tipo { get; set; } = TipoRegraComissao.PercentualSobreVenda;
    public decimal Percentual { get; set; }
    public decimal ValorFixo { get; set; }

    /// <summary>Valor mínimo da venda para aplicar esta regra (ex.: faixa/meta).</summary>
    public decimal? ValorMinimoVenda { get; set; }
    /// <summary>Quanto maior, maior a prioridade de aplicação.</summary>
    public int Prioridade { get; set; } = 1;
}
