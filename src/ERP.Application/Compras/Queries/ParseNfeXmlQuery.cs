using ERP.Application.Compras.Common;
using ERP.Application.Compras.DTOs;
using MediatR;

namespace ERP.Application.Compras.Queries;

public record ParseNfeXmlQuery(string XmlContent) : IRequest<NfeXmlParseResultDto>;

public class ParseNfeXmlQueryHandler : IRequestHandler<ParseNfeXmlQuery, NfeXmlParseResultDto>
{
    private readonly INfeXmlParserService _parserService;

    public ParseNfeXmlQueryHandler(INfeXmlParserService parserService)
    {
        _parserService = parserService;
    }

    public async Task<NfeXmlParseResultDto> Handle(ParseNfeXmlQuery request, CancellationToken cancellationToken)
    {
        return await _parserService.ParseNfeXmlAsync(request.XmlContent, cancellationToken);
    }
}
