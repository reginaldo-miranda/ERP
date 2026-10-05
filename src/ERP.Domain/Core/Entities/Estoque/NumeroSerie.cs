using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Estoque;

/// <summary>
/// Rastreamento individualizado de número de série por unidade de produto em depósito.
/// </summary>
public class NumeroSerie : BaseEmpresaEntity, IAggregateRoot
{
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public int DepositoId { get; set; }
    public Deposito Deposito { get; set; } = null!;

    public int? LoteId { get; set; }
    public Lote? Lote { get; set; }

    public string Numero { get; set; } = string.Empty;
    public StatusNumeroSerie Status { get; set; } = StatusNumeroSerie.Disponivel;

    public DateTime DataEntrada { get; set; } = DateTime.UtcNow;
    public DateTime? DataSaida { get; set; }
    public string? Observacoes { get; set; }
}
