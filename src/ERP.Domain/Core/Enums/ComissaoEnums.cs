namespace ERP.Domain.Core.Enums;

/// <summary>Forma de cálculo da comissão de venda.</summary>
public enum TipoRegraComissao
{
    PercentualSobreVenda = 1,
    ValorFixoPorVenda = 2
}

/// <summary>Ciclo de vida de uma comissão gerada sobre venda faturada.</summary>
public enum StatusComissao
{
    Pendente = 1,
    Aprovada = 2,
    Paga = 3,
    Cancelada = 4
}
