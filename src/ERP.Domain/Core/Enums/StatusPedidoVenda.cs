namespace ERP.Domain.Core.Enums;

/// <summary>
/// Ciclo de vida de um pedido de venda.
/// </summary>
public enum StatusPedidoVenda
{
    Pendente = 1,
    Aprovado = 2,
    Faturado = 3,
    Cancelado = 4
}
