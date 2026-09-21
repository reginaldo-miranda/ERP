using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities.Financeiro;

/// <summary>
/// Registro de recebimento (parcial ou total) de uma Conta a Receber.
/// </summary>
public class BaixaContaReceber : BaseEmpresaEntity
{
    public int ContaReceberId { get; set; }
    public ContaReceber ContaReceber { get; set; } = null!;

    public int ContaBancariaId { get; set; }
    public ContaBancaria ContaBancaria { get; set; } = null!;

    public int FormaPagamentoId { get; set; }
    public FormaPagamento FormaPagamento { get; set; } = null!;

    public DateTime DataBaixa { get; set; } = DateTime.UtcNow.Date;

    public decimal ValorPrincipal { get; set; } // Valor abatido do crédito
    public decimal ValorJuros { get; set; }
    public decimal ValorMulta { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotalRecebido { get; set; } // Valor efetivamente creditado no caixa/banco

    public string? Observacoes { get; set; }

    public int? MovimentacaoFinanceiraId { get; set; }
    public MovimentacaoFinanceira? MovimentacaoFinanceira { get; set; }
}
