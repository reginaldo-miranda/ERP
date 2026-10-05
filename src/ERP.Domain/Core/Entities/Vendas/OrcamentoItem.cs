using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Vendas;

/// <summary>
/// Item de orçamento: produto ou serviço, com preço editável e desconto por item.
/// </summary>
public class OrcamentoItem : BaseEmpresaEntity
{
    public int OrcamentoId { get; set; }
    public Orcamento Orcamento { get; set; } = null!;

    public TipoItemVenda TipoItem { get; set; }

    public int? ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    public int? ServicoId { get; set; }
    public Servico? Servico { get; set; }

    /// <summary>Descrição congelada no momento da venda (histórico imune a edição do cadastro).</summary>
    public string Descricao { get; set; } = string.Empty;

    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }

    public TipoDescontoVenda DescontoTipo { get; set; } = TipoDescontoVenda.Valor;
    public decimal Desconto { get; set; }
    /// <summary>Desconto efetivamente aplicado ao item, em R$.</summary>
    public decimal ValorDesconto { get; set; }

    public decimal SubTotal { get; set; }
}
