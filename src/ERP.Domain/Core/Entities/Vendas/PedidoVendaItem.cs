using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Vendas;

/// <summary>
/// Item de pedido de venda. <see cref="CustoUnitario"/> é preenchido no faturamento
/// (custo médio vigente do produto) e alimenta o CMV.
/// </summary>
public class PedidoVendaItem : BaseEmpresaEntity
{
    public int PedidoVendaId { get; set; }
    public PedidoVenda PedidoVenda { get; set; } = null!;

    public TipoItemVenda TipoItem { get; set; }

    public int? ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    public int? ServicoId { get; set; }
    public Servico? Servico { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }

    public TipoDescontoVenda DescontoTipo { get; set; } = TipoDescontoVenda.Valor;
    public decimal Desconto { get; set; }
    public decimal ValorDesconto { get; set; }

    public decimal SubTotal { get; set; }

    /// <summary>Custo unitário no momento do faturamento (0 para serviços ou antes de faturar).</summary>
    public decimal CustoUnitario { get; set; }
}
