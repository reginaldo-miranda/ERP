using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Compras;

/// <summary>
/// Solicitação interna de compra de materiais ou produtos para reposição.
/// </summary>
public class SolicitacaoCompra : BaseEmpresaEntity, IAggregateRoot
{
    /// <summary>Número sequencial formatado, ex.: SOL-2026-000001.</summary>
    public string Numero { get; set; } = string.Empty;

    public string SolicitanteId { get; set; } = string.Empty;
    public string SolicitanteNome { get; set; } = string.Empty;

    public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataNecessidade { get; set; }

    public StatusSolicitacaoCompra Status { get; set; } = StatusSolicitacaoCompra.Pendente;
    public PrioridadeSolicitacaoCompra Prioridade { get; set; } = PrioridadeSolicitacaoCompra.Normal;

    public string? Departamento { get; set; }
    public string? Justificativa { get; set; }
    public string? Observacoes { get; set; }

    public DateTime? DataAprovacao { get; set; }
    public string? AprovadorNome { get; set; }
    public string? MotivoRejeicao { get; set; }

    public ICollection<SolicitacaoCompraItem> Itens { get; set; } = new List<SolicitacaoCompraItem>();
}
