using ERP.Application.Vendas.Queries;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/vendas")]
[Authorize]
public class VendasRelatoriosController : ControllerBase
{
    private readonly IMediator _mediator;

    public VendasRelatoriosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("historico")]
    public async Task<IActionResult> GetHistorico(
        [FromQuery] string? busca,
        [FromQuery] int? clienteId,
        [FromQuery] string? vendedorId,
        [FromQuery] StatusPedidoVenda? status,
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetHistoricoVendasQuery(
            busca, clienteId, vendedorId, status, dataInicio, dataFim, pagina, tamanho), ct);
        return Ok(result);
    }

    [HttpGet("resumo")]
    public async Task<IActionResult> GetResumo(
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetResumoVendasQuery(dataInicio, dataFim), ct);
        return Ok(result);
    }
}
