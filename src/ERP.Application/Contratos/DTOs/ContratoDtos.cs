using ERP.Domain.Core.Enums;

namespace ERP.Application.Contratos.DTOs;

public class ContratoRecorrenteDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int? ServicoId { get; set; }
    public string? ServicoNome { get; set; }
    public int? FormaPagamentoId { get; set; }
    public string? FormaPagamentoNome { get; set; }
    public PeriodicidadeContrato Periodicidade { get; set; }
    public StatusContrato Status { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public int DiaVencimento { get; set; }
    public decimal ValorMensal { get; set; }
    public decimal PercentualReajustePadrao { get; set; }
    public string? IndiceReajuste { get; set; }
    public bool RenovacaoAutomatica { get; set; }
    public DateTime ProximaCobranca { get; set; }
    public DateTime? DataUltimoFaturamento { get; set; }
    public DateTime? DataUltimoReajuste { get; set; }
    public string? Observacoes { get; set; }
    public int TotalFaturamentos { get; set; }
    public bool CobrancaVencida => Status == StatusContrato.Ativo && ProximaCobranca.Date <= DateTime.UtcNow.Date;
}

public class ContratoDetalhesDto : ContratoRecorrenteDto
{
    public List<ContratoFaturamentoDto> Faturamentos { get; set; } = new();
    public List<ContratoReajusteDto> Reajustes { get; set; } = new();
}

public class ContratoFaturamentoDto
{
    public int Id { get; set; }
    public string Competencia { get; set; } = string.Empty;
    public DateTime DataReferencia { get; set; }
    public DateTime DataVencimento { get; set; }
    public decimal ValorFaturado { get; set; }
    public int? ContaReceberId { get; set; }
}

public class ContratoReajusteDto
{
    public int Id { get; set; }
    public DateTime DataReajuste { get; set; }
    public decimal ValorAnterior { get; set; }
    public decimal ValorNovo { get; set; }
    public decimal PercentualAplicado { get; set; }
    public string? Indice { get; set; }
    public string? Observacao { get; set; }
}

public class SalvarContratoDto
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int ClienteId { get; set; }
    public int? ServicoId { get; set; }
    public int? FormaPagamentoId { get; set; }
    public PeriodicidadeContrato Periodicidade { get; set; } = PeriodicidadeContrato.Mensal;
    public StatusContrato Status { get; set; } = StatusContrato.Ativo;
    public DateTime DataInicio { get; set; } = DateTime.Today;
    public DateTime? DataFim { get; set; }
    public int DiaVencimento { get; set; } = 10;
    public decimal ValorMensal { get; set; }
    public decimal PercentualReajustePadrao { get; set; }
    public string? IndiceReajuste { get; set; } = "IPCA";
    public bool RenovacaoAutomatica { get; set; } = true;
    public DateTime ProximaCobranca { get; set; } = DateTime.Today;
    public string? Observacoes { get; set; }
}

public class ContratoResumoKpiDto
{
    public int TotalAtivos { get; set; }
    public decimal MrrEstimado { get; set; }
    public int PendentesFaturamento { get; set; }
    public decimal FaturadoNoMes { get; set; }
}
