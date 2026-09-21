using ERP.Application.Financeiro.FluxoCaixa.Queries;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/fluxo-caixa")]
public class FluxoCaixaController : ControllerBase
{
    private readonly IMediator _mediator;

    public FluxoCaixaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        [FromQuery] int? contaBancariaId,
        [FromQuery] TipoAgrupamentoFluxoCaixa agrupamento = TipoAgrupamentoFluxoCaixa.Diario,
        [FromQuery] bool incluirProjetado = true,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetFluxoCaixaQuery(dataInicio, dataFim, contaBancariaId, agrupamento, incluirProjetado), ct);
        return Ok(result);
    }

    [HttpGet("csv")]
    public async Task<IActionResult> GetCsv(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        [FromQuery] int? contaBancariaId,
        [FromQuery] TipoAgrupamentoFluxoCaixa agrupamento = TipoAgrupamentoFluxoCaixa.Diario,
        [FromQuery] bool incluirProjetado = true,
        CancellationToken ct = default)
    {
        var bytes = await _mediator.Send(
            new ExportFluxoCaixaCsvQuery(dataInicio, dataFim, contaBancariaId, agrupamento, incluirProjetado), ct);
        return File(bytes, "text/csv", $"fluxo-caixa-{dataInicio:yyyyMMdd}-{dataFim:yyyyMMdd}.csv");
    }

    [HttpGet("pdf")]
    public async Task<IActionResult> GetPdf(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        [FromQuery] int? contaBancariaId,
        [FromQuery] TipoAgrupamentoFluxoCaixa agrupamento = TipoAgrupamentoFluxoCaixa.Diario,
        [FromQuery] bool incluirProjetado = true,
        CancellationToken ct = default)
    {
        var bytes = await _mediator.Send(
            new ExportFluxoCaixaPdfQuery(dataInicio, dataFim, contaBancariaId, agrupamento, incluirProjetado), ct);
        return File(bytes, "application/pdf", $"fluxo-caixa-{dataInicio:yyyyMMdd}-{dataFim:yyyyMMdd}.pdf");
    }
}
