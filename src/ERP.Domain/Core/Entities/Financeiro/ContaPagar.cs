using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Financeiro;

/// <summary>
/// Título de Conta a Pagar com suporte a parcelas e baixas parciais/totais.
/// </summary>
public class ContaPagar : BaseEmpresaEntity, IAggregateRoot
{
    public int? FornecedorId { get; set; }
    public Fornecedor? Fornecedor { get; set; }

    public string Descricao { get; set; } = string.Empty;
    public string? NumeroDocumento { get; set; } // NF, Fatura, Boleto

    public decimal ValorOriginal { get; set; }
    public decimal ValorPago { get; set; }
    public decimal SaldoRestante { get; set; }

    public DateTime DataEmissao { get; set; } = DateTime.UtcNow.Date;
    public DateTime DataVencimento { get; set; }
    public DateTime? DataCompetencia { get; set; }

    public StatusContaFinanceira Status { get; set; } = StatusContaFinanceira.Pendente;

    public int? FormaPagamentoId { get; set; }
    public FormaPagamento? FormaPagamento { get; set; }

    public int? ContaBancariaId { get; set; }
    public ContaBancaria? ContaBancaria { get; set; }

    public int? PlanoContaId { get; set; }
    public PlanoConta? PlanoConta { get; set; }

    public int? CentroCustoId { get; set; }
    public CentroCusto? CentroCusto { get; set; }

    public int NumeroParcela { get; set; } = 1;
    public int TotalParcelas { get; set; } = 1;
    public Guid? IdParcelamento { get; set; } // Agrupador de parcelas da mesma compra

    public string? Observacoes { get; set; }

    // Navegação
    public ICollection<BaixaContaPagar> Baixas { get; set; } = new List<BaixaContaPagar>();
}
