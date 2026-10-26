using ERP.Application.Compras.DTOs;

namespace ERP.Application.Compras.Common;

/// <summary>
/// Serviço responsável por interpretar o XML da NF-e (Layout 4.0 SEFAZ),
/// validar regras fiscais e estruturais, identificar o fornecedor, cruzar produtos
/// com o catálogo interno e sugerir pedidos de compras em aberto.
/// </summary>
public interface INfeXmlParserService
{
    Task<NfeXmlParseResultDto> ParseNfeXmlAsync(string xmlContent, CancellationToken cancellationToken = default);
}
