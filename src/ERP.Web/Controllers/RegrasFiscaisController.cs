using ERP.Application.Fiscal.Commands;
using ERP.Application.Fiscal.Queries;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/fiscal")]
[Authorize]
public class RegrasFiscaisController : ControllerBase
{
    private readonly IMediator _mediator;

    public RegrasFiscaisController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("regime")]
    public async Task<IActionResult> ObterRegime(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetRegimeTributarioQuery(), ct);
        return Ok(result);
    }

    [HttpPut("regime")]
    public async Task<IActionResult> SalvarRegime([FromBody] SalvarRegimeTributarioCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("regras")]
    public async Task<IActionResult> ListarRegras([FromQuery] TipoOperacaoFiscal? tipoOperacao, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetRegrasTributariasQuery(tipoOperacao), ct);
        return Ok(result);
    }

    [HttpPost("regras")]
    public async Task<IActionResult> SalvarRegra([FromBody] SalvarRegraTributariaCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("regras/{id:int}")]
    public async Task<IActionResult> ExcluirRegra(int id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ExcluirRegraTributariaCommand(id), ct);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("ncm")]
    public async Task<IActionResult> BuscarNcms([FromQuery] string? busca, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetNcmsQuery(busca), ct);
        return Ok(result);
    }

    [HttpGet("cfop")]
    public async Task<IActionResult> BuscarCfops([FromQuery] string? busca, [FromQuery] TipoOperacaoFiscal? tipoOperacao, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetCfopsQuery(busca, tipoOperacao), ct);
        return Ok(result);
    }
}
