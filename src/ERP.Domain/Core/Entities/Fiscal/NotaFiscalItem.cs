using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;

namespace ERP.Domain.Core.Entities.Fiscal;

/// <summary>
/// Item de mercadoria ou produto contido na Nota Fiscal Eletrônica.
/// </summary>
public class NotaFiscalItem : BaseEmpresaEntity
{
    public int NotaFiscalId { get; set; }
    public NotaFiscal NotaFiscal { get; set; } = null!;

    /// <summary>Número de ordem do item na nota (nItem: 1, 2, 3...).</summary>
    public int NumeroItem { get; set; }

    public int? ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    public string CodigoProduto { get; set; } = string.Empty;
    public string DescricaoProduto { get; set; } = string.Empty;
    public string Ncm { get; set; } = "00000000";
    public string Cfop { get; set; } = "5102";
    public string UnidadeMedida { get; set; } = "UN";

    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotalBruto { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorSeguro { get; set; }
    public decimal ValorOutrasDespesas { get; set; }
    public decimal ValorTotalLiquido { get; set; }

    // --- Tributação ICMS ---
    /// <summary>Origem da mercadoria (0=Nacional, 1=Estrangeira importação direta, 2=Estrangeira adquirida internamente...).</summary>
    public int OrigemMercadoria { get; set; } = 0;

    /// <summary>CST de ICMS para empresas do Regime Normal (ex.: "00", "10", "20", "60").</summary>
    public string? CstIcms { get; set; }

    /// <summary>CSOSN para empresas do Simples Nacional (ex.: "101", "102", "500").</summary>
    public string? Csosn { get; set; }

    public decimal BaseCalculoIcms { get; set; }
    public decimal AliquotaIcms { get; set; }
    public decimal ValorIcms { get; set; }
    public decimal PercentualReducaoBaseIcms { get; set; }

    // ICMS ST
    public decimal BaseCalculoIcmsSt { get; set; }
    public decimal AliquotaIcmsSt { get; set; }
    public decimal ValorIcmsSt { get; set; }
    public decimal PercentualMvaSt { get; set; }

    // --- Tributação PIS ---
    public string? CstPis { get; set; }
    public decimal BaseCalculoPis { get; set; }
    public decimal AliquotaPis { get; set; }
    public decimal ValorPis { get; set; }

    // --- Tributação COFINS ---
    public string? CstCofins { get; set; }
    public decimal BaseCalculoCofins { get; set; }
    public decimal AliquotaCofins { get; set; }
    public decimal ValorCofins { get; set; }

    // --- Tributação IPI ---
    public string? CstIpi { get; set; }
    public decimal BaseCalculoIpi { get; set; }
    public decimal AliquotaIpi { get; set; }
    public decimal ValorIpi { get; set; }

    // --- Reforma Tributária (IBS / CBS) ---
    public decimal AliquotaIbs { get; set; }
    public decimal ValorIbs { get; set; }
    public decimal AliquotaCbs { get; set; }
    public decimal ValorCbs { get; set; }

    // Lei 12.741/2012
    public decimal ValorAproximadoTributos { get; set; }
}
