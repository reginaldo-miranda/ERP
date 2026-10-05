using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;

namespace ERP.Domain.Core.Entities.Estoque;

/// <summary>
/// Representa um lote de fabricação de um produto com controle de validade e saldo em um depósito.
/// </summary>
public class Lote : BaseEmpresaEntity, IAggregateRoot
{
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public int DepositoId { get; set; }
    public Deposito Deposito { get; set; } = null!;

    public string CodigoLote { get; set; } = string.Empty;
    public DateTime? DataFabricacao { get; set; }
    public DateTime DataValidade { get; set; }

    public decimal QuantidadeInicial { get; set; }
    public decimal QuantidadeAtual { get; set; }
    public decimal CustoUnitario { get; set; }

    public bool Ativo { get; set; } = true;
    public string? Observacoes { get; set; }

    // Propriedades calculadas
    public bool Vencido => DataValidade.Date < DateTime.UtcNow.Date;
    public int DiasParaVencer => (DataValidade.Date - DateTime.UtcNow.Date).Days;
}
