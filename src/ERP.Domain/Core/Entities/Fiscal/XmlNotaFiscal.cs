using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Fiscal;

/// <summary>
/// Histórico e armazenamento de arquivos XML gerados ou recebidos no ciclo fiscal.
/// </summary>
public class XmlNotaFiscal : BaseEmpresaEntity
{
    public int NotaFiscalId { get; set; }
    public NotaFiscal NotaFiscal { get; set; } = null!;

    /// <summary>Tipo do arquivo XML (Envio, Retorno, Cancelamento, CartaCorrecao, DistribuicaoProc).</summary>
    public TipoXmlNotaFiscal TipoXml { get; set; } = TipoXmlNotaFiscal.Envio;

    /// <summary>Conteúdo integral em texto estruturado XML.</summary>
    public string XmlConteudo { get; set; } = string.Empty;

    /// <summary>Chave de acesso vinculada no momento da geração do XML.</summary>
    public string? ChaveAcesso { get; set; }

    /// <summary>Data e hora em que o XML foi persistido.</summary>
    public DateTime DataGeracao { get; set; } = DateTime.UtcNow;
}
