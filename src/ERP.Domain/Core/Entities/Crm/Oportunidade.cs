using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Entities.Vendas;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Crm;

/// <summary>
/// Oportunidade comercial acompanhada no funil de vendas (Kanban/Lista).
/// Pode estar vinculada a um Lead ou diretamente a um Cliente existente,
/// e gerar Orçamento ou Pedido de Venda com 1 clique.
/// </summary>
public class Oportunidade : BaseEmpresaEntity, IAggregateRoot
{
    /// <summary>Código sequencial por empresa/ano, ex.: CRM-2026-0001.</summary>
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    public int? LeadId { get; set; }
    public Lead? Lead { get; set; }

    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public EtapaFunil Etapa { get; set; } = EtapaFunil.Prospeccao;
    public decimal ValorEstimado { get; set; }
    /// <summary>Probabilidade de fechamento (0 a 100).</summary>
    public int Probabilidade { get; set; } = 20;

    public DateTime? DataFechamentoPrevista { get; set; }
    public DateTime? DataFechamentoReal { get; set; }

    /// <summary>Id do usuário (Identity) vendedor responsável pela oportunidade.</summary>
    public string? ResponsavelId { get; set; }
    public string? ResponsavelNome { get; set; }

    public string? MotivoPerda { get; set; }

    /// <summary>Orçamento gerado a partir desta oportunidade.</summary>
    public int? OrcamentoId { get; set; }
    public Orcamento? Orcamento { get; set; }

    /// <summary>Pedido de Venda gerado a partir desta oportunidade.</summary>
    public int? PedidoVendaId { get; set; }
    public PedidoVenda? PedidoVenda { get; set; }

    public ICollection<AtividadeCrm> Atividades { get; set; } = new List<AtividadeCrm>();
}
