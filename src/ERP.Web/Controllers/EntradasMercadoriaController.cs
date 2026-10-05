using ERP.Application.Compras.Commands;
using ERP.Application.Compras.DTOs;
using ERP.Application.Compras.Queries;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/entradas-mercadoria")]
[Authorize]
public class EntradasMercadoriaController : ControllerBase
{
    private readonly IMediator _mediator;

    public EntradasMercadoriaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca,
        [FromQuery] int? fornecedorId,
        [FromQuery] int? depositoId,
        [FromQuery] StatusEntradaMercadoria? status,
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 50)
    {
        var result = await _mediator.Send(new GetEntradasMercadoriaQuery(
            busca, fornecedorId, depositoId, status, dataInicio, dataFim, pagina, tamanhoPagina));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var result = await _mediator.Send(new GetEntradaMercadoriaByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarEntradaMercadoriaDto dto)
    {
        var id = await _mediator.Send(new RegistrarEntradaMercadoriaCommand(dto));
        return CreatedAtAction(nameof(ObterPorId), new { id }, new { id });
    }

    [HttpPost("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id, [FromBody] string motivo)
    {
        var sucesso = await _mediator.Send(new CancelarEntradaMercadoriaCommand(id, motivo));
        return Ok(new { sucesso });
    }
}
