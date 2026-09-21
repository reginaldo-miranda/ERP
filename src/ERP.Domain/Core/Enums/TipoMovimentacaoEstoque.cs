namespace ERP.Domain.Core.Enums;

/// <summary>
/// Tipo de movimentação de estoque.
/// </summary>
public enum TipoMovimentacaoEstoque
{
    Entrada = 1,
    Saida = 2,
    Transferencia = 3,
    AjustePositivo = 4,
    AjusteNegativo = 5
}
