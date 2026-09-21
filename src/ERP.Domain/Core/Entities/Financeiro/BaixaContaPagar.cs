using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities.Financeiro;

/// <summary>
/// Registro de quitação (parcial ou total) de uma Conta a Pagar.
/// </summary>
public class BaixaContaPagar : BaseEmpresaEntity
{
    public int ContaPagarId { get; set; }
    public ContaPagar ContaPagar { get; set; } = null!;

    public int ContaBancariaId { get; set; }
    public ContaBancaria ContaBancaria { get; set; } = null!;

    public int FormaPagamentoId { get; set; }
    public FormaPagamento FormaPagamento { get; set; } = null!;

    public DateTime DataBaixa { get; set; } = DateTime.UtcNow.Date;

    public decimal ValorPrincipal { get; set; } // Valor abatido da dívida
    public decimal ValorJuros { get; set; }
    public decimal ValorMulta { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotalPago { get; set; } // Valor efetivamente debitado do caixa/banco

    public string? Observacoes { get; set; }

    public int? MovimentacaoFinanceiraId { get; set; }
    public MovimentacaoFinanceira? MovimentacaoFinanceira { get; set; }
}
