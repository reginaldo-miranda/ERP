using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;

namespace ERP.Domain.Core.Entities.Estoque;

/// <summary>
/// Saldo e custeio de um produto em um depósito específico.
/// </summary>
public class EstoqueProduto : BaseEmpresaEntity
{
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public int DepositoId { get; set; }
    public Deposito Deposito { get; set; } = null!;

    public decimal Quantidade { get; set; } = 0;
    public decimal CustoMedio { get; set; } = 0;
    public decimal CustoUltimaCompra { get; set; } = 0;
    public decimal EstoqueMinimo { get; set; } = 0;
    public decimal EstoqueMaximo { get; set; } = 0;
}
