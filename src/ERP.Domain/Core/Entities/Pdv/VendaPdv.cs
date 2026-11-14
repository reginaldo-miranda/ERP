using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Entities.Comissoes;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Pdv;

/// <summary>
/// Venda ou Pré-Venda realizada no PDV.
/// </summary>
public class VendaPdv : BaseEmpresaEntity, IAggregateRoot
{
    public string Numero { get; set; } = string.Empty;

    public int TerminalPdvId { get; set; }
    public TerminalPdv TerminalPdv { get; set; } = null!;

    public int? SessaoCaixaPdvId { get; set; }
    public SessaoCaixaPdv? SessaoCaixaPdv { get; set; }

    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public string? ConsumidorNome { get; set; }
    public string? ConsumidorCpf { get; set; }

    public int? VendedorId { get; set; }
    public Vendedor? Vendedor { get; set; }
    public string? VendedorNome { get; set; }

    public StatusVendaPdv Status { get; set; } = StatusVendaPdv.Finalizada;
    public DateTime DataVenda { get; set; } = DateTime.UtcNow;

    public decimal ValorBruto { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal Troco { get; set; }
    public string? Observacoes { get; set; }

    public ICollection<VendaPdvItem> Itens { get; set; } = new List<VendaPdvItem>();
    public ICollection<VendaPdvPagamento> Pagamentos { get; set; } = new List<VendaPdvPagamento>();
}

public class VendaPdvItem : BaseEmpresaEntity
{
    public int VendaPdvId { get; set; }
    public VendaPdv VendaPdv { get; set; } = null!;

    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public string CodigoProduto { get; set; } = string.Empty;
    public string NomeProduto { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }
}

public class VendaPdvPagamento : BaseEmpresaEntity
{
    public int VendaPdvId { get; set; }
    public VendaPdv VendaPdv { get; set; } = null!;

    public int? FormaPagamentoId { get; set; }
    public FormaPagamento? FormaPagamento { get; set; }

    public string DescricaoForma { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
