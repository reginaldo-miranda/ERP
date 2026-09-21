using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Financeiro;

/// <summary>
/// Movimentação financeira de débito ou crédito na conta bancária/caixa.
/// </summary>
public class MovimentacaoFinanceira : BaseEmpresaEntity, IAggregateRoot
{
    public int ContaBancariaId { get; set; }
    public ContaBancaria ContaBancaria { get; set; } = null!;

    public TipoOperacaoFinanceira Tipo { get; set; } = TipoOperacaoFinanceira.Entrada;
    public DateTime DataMovimentacao { get; set; } = DateTime.UtcNow.Date;
    public decimal Valor { get; set; }
    public string Descricao { get; set; } = string.Empty;

    public int? PlanoContaId { get; set; }
    public PlanoConta? PlanoConta { get; set; }

    public int? CentroCustoId { get; set; }
    public CentroCusto? CentroCusto { get; set; }

    public int? ContaPagarId { get; set; }
    public ContaPagar? ContaPagar { get; set; }

    public int? ContaReceberId { get; set; }
    public ContaReceber? ContaReceber { get; set; }

    public string? DocumentoReferencia { get; set; }
    public string? Observacoes { get; set; }
}
