using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities.Contratos;

/// <summary>
/// Histórico de reajustes aplicados ao valor de um Contrato Recorrente.
/// </summary>
public class ContratoReajuste : BaseEmpresaEntity
{
    public int ContratoRecorrenteId { get; set; }
    public ContratoRecorrente ContratoRecorrente { get; set; } = null!;

    public DateTime DataReajuste { get; set; } = DateTime.UtcNow;
    public decimal ValorAnterior { get; set; }
    public decimal ValorNovo { get; set; }
    public decimal PercentualAplicado { get; set; }
    public string? Indice { get; set; }
    public string? Observacao { get; set; }
}
