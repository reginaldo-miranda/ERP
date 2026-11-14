using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Pdv;

/// <summary>
/// Turno / Sessão de caixa independente de um Terminal PDV.
/// </summary>
public class SessaoCaixaPdv : BaseEmpresaEntity, IAggregateRoot
{
    public int TerminalPdvId { get; set; }
    public TerminalPdv TerminalPdv { get; set; } = null!;

    public string? OperadorId { get; set; }
    public string OperadorNome { get; set; } = string.Empty;

    public DateTime DataAbertura { get; set; } = DateTime.UtcNow;
    public decimal ValorAbertura { get; set; }

    public DateTime? DataFechamento { get; set; }
    public decimal? ValorFechamentoInformado { get; set; }
    public decimal? ValorFechamentoCalculado { get; set; }
    public decimal? DiferencaQuebra { get; set; }

    public StatusSessaoCaixa Status { get; set; } = StatusSessaoCaixa.Aberta;
    public string? ObservacoesFechamento { get; set; }

    public ICollection<MovimentoCaixaPdv> Movimentos { get; set; } = new List<MovimentoCaixaPdv>();
    public ICollection<VendaPdv> Vendas { get; set; } = new List<VendaPdv>();
}
