using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Servicos;

/// <summary>
/// Representa uma Ordem de Serviço (OS) para assistência técnica, manutenção ou prestação de serviços.
/// </summary>
public class OrdemServico : BaseEmpresaEntity, IAggregateRoot
{
    /// <summary>Número sequencial único por empresa/ano, ex.: OS-2026-0001.</summary>
    public string Numero { get; set; } = string.Empty;

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public StatusOrdemServico Status { get; set; } = StatusOrdemServico.Aberta;
    public PrioridadeOrdemServico Prioridade { get; set; } = PrioridadeOrdemServico.Normal;

    public DateTime DataAbertura { get; set; } = DateTime.UtcNow;
    public DateTime? PrevisaoConclusao { get; set; }
    public DateTime? DataConclusao { get; set; }
    public DateTime? DataEntrega { get; set; }

    public string? TecnicoResponsavel { get; set; }

    // Identificação do Equipamento / Objeto
    public string Equipamento { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? NumeroSerie { get; set; }
    public string? Acessorios { get; set; }
    public string? CondicoesGerais { get; set; }

    // Relato e Parecer Técnico
    public string DefeitoRelatado { get; set; } = string.Empty;
    public string? LaudoTecnico { get; set; }
    public string? SolucaoTecnica { get; set; }

    // Valores Financeiros
    public decimal ValorServicos { get; set; }
    public decimal ValorPecas { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }

    // Condições de Pagamento e Faturamento
    public int? FormaPagamentoId { get; set; }
    public FormaPagamento? FormaPagamento { get; set; }
    public int NumeroParcelas { get; set; } = 1;
    public Guid? IdParcelamento { get; set; }

    public string? Observacoes { get; set; }
    public string? TermoGarantia { get; set; } = "Garantia de 90 dias para as peças aplicadas e serviços executados, contados a partir da data de entrega.";

    public DateTime? DataCancelamento { get; set; }
    public string? MotivoCancelamento { get; set; }

    // Itens
    public ICollection<OrdemServicoPeca> ItensPecas { get; set; } = new List<OrdemServicoPeca>();
    public ICollection<OrdemServicoServico> ItensServicos { get; set; } = new List<OrdemServicoServico>();
    public ICollection<OrdemServicoHistorico> HistoricoStatus { get; set; } = new List<OrdemServicoHistorico>();

    public void RecalcularTotais()
    {
        ValorPecas = ItensPecas.Sum(p => p.ValorTotal);
        ValorServicos = ItensServicos.Sum(s => s.ValorTotal);
        ValorTotal = Math.Max(0, (ValorPecas + ValorServicos) - ValorDesconto);
    }
}

