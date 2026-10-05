namespace ERP.Domain.Core.Enums;

/// <summary>
/// Status persistido de um orçamento de venda.
/// O estado "Expirado" não é persistido: é calculado na consulta a partir de DataValidade.
/// </summary>
public enum StatusOrcamento
{
    Aberto = 1,
    Convertido = 2,
    Rejeitado = 3,
    Cancelado = 4
}
