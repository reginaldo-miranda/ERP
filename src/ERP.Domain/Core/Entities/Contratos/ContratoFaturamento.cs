using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Financeiro;

namespace ERP.Domain.Core.Entities.Contratos;

/// <summary>
/// Registro de cada parcela/ciclo faturado de um Contrato Recorrente e seu vínculo com o Contas a Receber.
/// </summary>
public class ContratoFaturamento : BaseEmpresaEntity
{
    public int ContratoRecorrenteId { get; set; }
    public ContratoRecorrente ContratoRecorrente { get; set; } = null!;

    public string Competencia { get; set; } = string.Empty;
    public DateTime DataReferencia { get; set; }
    public DateTime DataVencimento { get; set; }
    public decimal ValorFaturado { get; set; }

    public int? ContaReceberId { get; set; }
    public ContaReceber? ContaReceber { get; set; }
}
