using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities.Fiscal;

/// <summary>
/// Nomenclatura Comum do Mercosul (NCM) para classificação fiscal de mercadorias.
/// Tabela universal compartilhada entre empresas do ERP.
/// </summary>
public class Ncm : BaseAuditableEntity
{
    /// <summary>Código NCM de 8 dígitos, ex.: "84713012".</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Descrição técnica oficial da mercadoria na tabela TIPI/NCM.</summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Alíquota padrão de IPI para o NCM (%).</summary>
    public decimal AliquotaIpi { get; set; }

    /// <summary>Alíquota padrão do Imposto de Importação (II) (%).</summary>
    public decimal AliquotaII { get; set; }

    /// <summary>Alíquota estimada de tributos federais nacionais (IBPT / Lei 12.741).</summary>
    public decimal AliquotaNacional { get; set; }

    /// <summary>Alíquota estimada de tributos federais importados (IBPT / Lei 12.741).</summary>
    public decimal AliquotaImportado { get; set; }

    /// <summary>Alíquota estimada de tributos estaduais (IBPT / Lei 12.741).</summary>
    public decimal AliquotaEstadual { get; set; }

    /// <summary>Alíquota estimada de tributos municipais (IBPT / Lei 12.741).</summary>
    public decimal AliquotaMunicipal { get; set; }
}
