namespace ERP.Domain.Core.Enums;

/// <summary>
/// Tipos de documentos XML transacionados no ciclo fiscal.
/// </summary>
public enum TipoXmlNotaFiscal
{
    /// <summary>XML de envio do lote / da nota assinada</summary>
    Envio = 1,

    /// <summary>XML de retorno / recibo / autorização da SEFAZ</summary>
    Retorno = 2,

    /// <summary>XML de pedido ou homologação de cancelamento</summary>
    Cancelamento = 3,

    /// <summary>XML de Carta de Correção Eletrônica (CC-e)</summary>
    CartaCorrecao = 4,

    /// <summary>XML de pedido ou homologação de inutilização de numeração</summary>
    Inutilizacao = 5,

    /// <summary>XML completo de distribuição processada (-procNFe.xml)</summary>
    DistribuicaoProc = 6
}
