namespace ERP.Domain.Core.Enums;

/// <summary>
/// Origem da movimentação de estoque.
/// </summary>
public enum OrigemMovimentacaoEstoque
{
    Manual = 1,
    Compra = 2,
    Venda = 3,
    Inventario = 4,
    Devolucao = 5
}
