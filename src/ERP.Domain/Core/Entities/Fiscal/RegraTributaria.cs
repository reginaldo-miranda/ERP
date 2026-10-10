using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Fiscal;

/// <summary>
/// Regra Tributária flexível por empresa, associando operação/NCM/UF com tributação de ICMS, PIS, COFINS, IPI e IBS/CBS.
/// </summary>
public class RegraTributaria : BaseEmpresaEntity, IAggregateRoot
{
    /// <summary>Identificação amigável da regra, ex: "Venda Interna SP - Simples Nacional sem ST".</summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Tipo de Operação (Saída ou Entrada).</summary>
    public TipoOperacaoFiscal TipoOperacao { get; set; } = TipoOperacaoFiscal.Saida;

    /// <summary>Código NCM específico a que se aplica a regra (se nulo, aplica-se a qualquer NCM que case o CFOP).</summary>
    public string? NcmCodigo { get; set; }

    /// <summary>Código CFOP padrão aplicado na operação (4 dígitos, ex.: "5102").</summary>
    public string CfopCodigo { get; set; } = "5102";

    /// <summary>UF do emitente/origem (ex: "SP", ou "**" para qualquer UF).</summary>
    public string UfOrigem { get; set; } = "**";

    /// <summary>UF do destinatário (ex: "SP", ou "**" para qualquer UF).</summary>
    public string UfDestino { get; set; } = "**";

    // --- ICMS ---
    /// <summary>CST de ICMS para empresas do Regime Normal (ex.: "00", "10", "20", "60", "90").</summary>
    public string? CstIcms { get; set; }

    /// <summary>CSOSN para empresas optantes pelo Simples Nacional (ex.: "101", "102", "500", "900").</summary>
    public string? Csosn { get; set; }

    /// <summary>Alíquota de ICMS própria (%).</summary>
    public decimal AliquotaIcms { get; set; }

    /// <summary>Percentual de redução de base de cálculo do ICMS (%).</summary>
    public decimal ReducaoBaseIcms { get; set; }

    /// <summary>Alíquota de ICMS com Substituição Tributária (%).</summary>
    public decimal AliquotaIcmsSt { get; set; }

    /// <summary>Percentual de Margem de Valor Agregado (MVA) para ICMS-ST (%).</summary>
    public decimal MvaIcmsSt { get; set; }

    /// <summary>Percentual de redução da base de cálculo do ICMS-ST (%).</summary>
    public decimal ReducaoBaseIcmsSt { get; set; }

    // --- PIS ---
    /// <summary>CST de PIS (ex.: "01", "07", "08", "49", "99").</summary>
    public string? CstPis { get; set; }

    /// <summary>Alíquota de PIS (%).</summary>
    public decimal AliquotaPis { get; set; }

    // --- COFINS ---
    /// <summary>CST de COFINS (ex.: "01", "07", "08", "49", "99").</summary>
    public string? CstCofins { get; set; }

    /// <summary>Alíquota de COFINS (%).</summary>
    public decimal AliquotaCofins { get; set; }

    // --- IPI ---
    /// <summary>CST de IPI (ex.: "50", "51", "52", "99").</summary>
    public string? CstIpi { get; set; }

    /// <summary>Alíquota de IPI (%).</summary>
    public decimal AliquotaIpi { get; set; }

    // --- Reforma Tributária (IBS / CBS) ---
    /// <summary>Alíquota estimada de IBS (Imposto sobre Bens e Serviços).</summary>
    public decimal AliquotaIbs { get; set; }

    /// <summary>Alíquota estimada de CBS (Contribuição sobre Bens e Serviços).</summary>
    public decimal AliquotaCbs { get; set; }

    /// <summary>Texto padrão a ser anexado em Informações Complementares da NF-e quando esta regra for acionada.</summary>
    public string? ObservacoesFiscais { get; set; }
}
