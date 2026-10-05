namespace ERP.Domain.Core.Enums;

/// <summary>
/// Status do ciclo de vida da Solicitação Interna de Compra.
/// </summary>
public enum StatusSolicitacaoCompra
{
    Pendente = 1,
    Aprovada = 2,
    Rejeitada = 3,
    Cotada = 4,
    ConvertidaPedido = 5,
    Cancelada = 6
}
