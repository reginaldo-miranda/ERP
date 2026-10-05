using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Vendas;

/// <summary>
/// Orçamento de venda para um cliente. Pode ser convertido em Pedido de Venda.
/// O estado "Expirado" não é persistido: é derivado de <see cref="DataValidade"/> nas consultas.
/// </summary>
public class Orcamento : BaseEmpresaEntity, IAggregateRoot
{
    /// <summary>Número sequencial por empresa/ano, ex.: ORC-2026-0001.</summary>
    public string Numero { get; set; } = string.Empty;

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    /// <summary>Id do usuário (Identity) que atua como vendedor.</summary>
    public string? VendedorId { get; set; }
    public string? VendedorNome { get; set; }

    public DateTime DataOrcamento { get; set; } = DateTime.UtcNow;
    public DateTime DataValidade { get; set; }
    public StatusOrcamento Status { get; set; } = StatusOrcamento.Aberto;

    /// <summary>Soma dos subtotais dos itens (já líquidos de desconto por item).</summary>
    public decimal ValorBruto { get; set; }

    public TipoDescontoVenda DescontoTipo { get; set; } = TipoDescontoVenda.Valor;
    /// <summary>Desconto geral informado pelo usuário (valor em R$ ou percentual, conforme <see cref="DescontoTipo"/>).</summary>
    public decimal DescontoGeral { get; set; }
    /// <summary>Desconto geral efetivamente aplicado, em R$.</summary>
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }

    public string? Observacao { get; set; }

    /// <summary>Preenchido quando o orçamento é convertido em pedido.</summary>
    public int? PedidoVendaId { get; set; }
    public PedidoVenda? PedidoVenda { get; set; }

    public ICollection<OrcamentoItem> Itens { get; set; } = new List<OrcamentoItem>();
}
