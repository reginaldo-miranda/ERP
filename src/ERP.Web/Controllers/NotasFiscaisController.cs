using ERP.Application.Fiscal.Commands;
using ERP.Application.Fiscal.DTOs;
using ERP.Application.Fiscal.Queries;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/notas-fiscais")]
[Authorize]
public class NotasFiscaisController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotasFiscaisController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca,
        [FromQuery] ModeloDocumentoFiscal? modelo,
        [FromQuery] StatusNotaFiscal? status,
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetNotasFiscaisQuery(
            busca, modelo, status, dataInicio, dataFim, pagina, tamanho), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterDetalhes(int id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetNotaFiscalDetalhesQuery(id), ct);
        return Ok(result);
    }

    [HttpPost("emitir")]
    public async Task<IActionResult> Emitir([FromBody] EmitirNotaFiscalCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.Sucesso)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id, [FromBody] CancelarNotaFiscalRequest request, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new CancelarNotaFiscalCommand(id, request.Justificativa), ct);
        if (!result.Sucesso)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:int}/cce")]
    public async Task<IActionResult> CartaCorrecao(int id, [FromBody] CartaCorrecaoRequest request, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new EnviarCartaCorrecaoCommand(id, request.CorrecaoTexto), ct);
        if (!result.Sucesso)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{id:int}/danfe")]
    public async Task<IActionResult> GerarDanfe(int id, CancellationToken ct = default)
    {
        var bytes = await _mediator.Send(new GerarDanfePdfQuery(id), ct);
        return File(bytes, "application/pdf", $"DANFE_NF_{id}.pdf");
    }

    [HttpGet("{id:int}/xml")]
    public async Task<IActionResult> DownloadXml(int id, [FromQuery] TipoXmlNotaFiscal? tipo, CancellationToken ct = default)
    {
        var xmlConteudo = await _mediator.Send(new GetXmlNotaFiscalQuery(id, tipo), ct);
        var bytes = System.Text.Encoding.UTF8.GetBytes(xmlConteudo);
        return File(bytes, "application/xml", $"NFe_{id}.xml");
    }

    [HttpGet("status-sefaz")]
    public async Task<IActionResult> ConsultarStatusSefaz(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ConsultarStatusSefazQuery(), ct);
        return Ok(result);
    }
}

public class CancelarNotaFiscalRequest
{
    public string Justificativa { get; set; } = string.Empty;
}

public class CartaCorrecaoRequest
{
    public string CorrecaoTexto { get; set; } = string.Empty;
}
