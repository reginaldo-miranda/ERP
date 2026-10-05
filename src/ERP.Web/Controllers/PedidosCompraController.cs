using ERP.Application.Compras.Commands;
using ERP.Application.Compras.DTOs;
using ERP.Application.Compras.Queries;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/pedidos-compra")]
[Authorize]
public class PedidosCompraController : ControllerBase
{
    private readonly IMediator _mediator;

    public PedidosCompraController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca,
        [FromQuery] int? fornecedorId,
        [FromQuery] StatusPedidoCompra? status,
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 50)
    {
        var result = await _mediator.Send(new GetPedidosCompraQuery(
            busca, fornecedorId, status, dataInicio, dataFim, pagina, tamanhoPagina));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var result = await _mediator.Send(new GetPedidoCompraByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarPedidoCompraDto dto)
    {
        var id = await _mediator.Send(new CriarPedidoCompraCommand(dto));
        return CreatedAtAction(nameof(ObterPorId), new { id }, new { id });
    }

    [HttpPost("{id:int}/aprovar")]
    public async Task<IActionResult> Aprovar(int id)
    {
        var sucesso = await _mediator.Send(new AprovarPedidoCompraCommand(id));
        return Ok(new { sucesso });
    }

    [HttpPost("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id, [FromBody] string motivo)
    {
        var sucesso = await _mediator.Send(new CancelarPedidoCompraCommand(id, motivo));
        return Ok(new { sucesso });
    }

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> GerarPdf(int id)
    {
        var pdfBytes = await _mediator.Send(new GerarPedidoCompraPdfQuery(id));
        return File(pdfBytes, "application/pdf", $"PedidoCompra_{id}.pdf");
    }
}
