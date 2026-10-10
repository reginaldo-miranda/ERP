namespace ERP.Domain.Core.Enums;

/// <summary>
/// Tipo de operação da Nota Fiscal (tpNF: 0=Entrada, 1=Saída).
/// </summary>
public enum TipoOperacaoFiscal
{
    /// <summary>0 - Entrada de mercadoria / devolução / importação</summary>
    Entrada = 0,

    /// <summary>1 - Saída de mercadoria / venda / remessa / exportação</summary>
    Saida = 1
}
