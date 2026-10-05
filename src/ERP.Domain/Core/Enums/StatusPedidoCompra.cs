namespace ERP.Domain.Core.Enums;

/// <summary>
/// Status do ciclo de vida do Pedido de Compra com fornecedor.
/// </summary>
public enum StatusPedidoCompra
{
    Pendente = 1,
    Aprovado = 2,
    ParcialmenteRecebido = 3,
    Recebido = 4,
    Cancelado = 5
}
