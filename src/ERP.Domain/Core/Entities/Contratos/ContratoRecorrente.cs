using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Contratos;

/// <summary>
/// Contrato recorrente com faturamento periódico automático no Contas a Receber,
/// suporte a renovação e histórico de reajustes.
/// </summary>
public class ContratoRecorrente : BaseEmpresaEntity, IAggregateRoot
{
    /// <summary>Número sequencial por empresa/ano, ex.: CTR-2026-0001.</summary>
    public string Numero { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int? ServicoId { get; set; }
    public Servico? Servico { get; set; }

    public int? FormaPagamentoId { get; set; }
    public FormaPagamento? FormaPagamento { get; set; }

    public PeriodicidadeContrato Periodicidade { get; set; } = PeriodicidadeContrato.Mensal;
    public StatusContrato Status { get; set; } = StatusContrato.Ativo;

    public DateTime DataInicio { get; set; } = DateTime.UtcNow.Date;
    public DateTime? DataFim { get; set; }
    /// <summary>Dia do mês (1 a 28) para vencimento da cobrança gerada.</summary>
    public int DiaVencimento { get; set; } = 10;

    public decimal ValorMensal { get; set; }
    public decimal PercentualReajustePadrao { get; set; }
    public string? IndiceReajuste { get; set; } = "IPCA";

    public bool RenovacaoAutomatica { get; set; } = true;
    public DateTime ProximaCobranca { get; set; } = DateTime.UtcNow.Date;
    public DateTime? DataUltimoFaturamento { get; set; }
    public DateTime? DataUltimoReajuste { get; set; }

    public string? Observacoes { get; set; }

    public ICollection<ContratoFaturamento> Faturamentos { get; set; } = new List<ContratoFaturamento>();
    public ICollection<ContratoReajuste> Reajustes { get; set; } = new List<ContratoReajuste>();
}
