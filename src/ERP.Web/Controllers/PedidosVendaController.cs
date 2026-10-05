using ERP.Application.Vendas.Commands;
using ERP.Application.Vendas.DTOs;
using ERP.Application.Vendas.Queries;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/pedidos-venda")]
[Authorize]
public class PedidosVendaController : ControllerBase
{
    private readonly IMediator _mediator;

    public PedidosVendaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? busca,
        [FromQuery] int? clienteId,
        [FromQuery] int? depositoId,
        [FromQuery] StatusPedidoVenda? status,
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetPedidosVendaQuery(
            busca, clienteId, depositoId, status, dataInicio, dataFim, pagina, tamanho), ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetPedidoVendaByIdQuery(id), ct);
        return result.Sucesso ? Ok(result.Dados) : NotFound(result.Erros);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CriarPedidoVendaDto dto, CancellationToken ct = default)
    {
        var id = await _mediator.Send(new CriarPedidoVendaCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AtualizarPedidoVendaDto dto, CancellationToken ct = default)
    {
        var sucesso = await _mediator.Send(new AtualizarPedidoVendaCommand(id, dto), ct);
        return sucesso ? NoContent() : BadRequest();
    }

    [HttpPost("{id}/aprovar")]
    public async Task<IActionResult> Aprovar(int id, CancellationToken ct = default)
    {
        var sucesso = await _mediator.Send(new AprovarPedidoVendaCommand(id), ct);
        return sucesso ? Ok() : BadRequest();
    }

    [HttpPost("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id, [FromBody] string? motivo, CancellationToken ct = default)
    {
        var sucesso = await _mediator.Send(new CancelarPedidoVendaCommand(id, motivo), ct);
        return sucesso ? Ok() : BadRequest();
    }

    [HttpPost("{id}/faturar")]
    public async Task<IActionResult> Faturar(int id, [FromBody] FaturarPedidoDto? dto, CancellationToken ct = default)
    {
        var comando = dto ?? new FaturarPedidoDto(id, null, null, null, null, null, null);
        if (comando.PedidoVendaId != id)
            comando = comando with { PedidoVendaId = id };

        var faturamentoId = await _mediator.Send(new FaturarPedidoVendaCommand(comando), ct);
        return Ok(new { faturamentoId });
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GerarPdf(int id, CancellationToken ct = default)
    {
        var bytes = await _mediator.Send(new GerarPedidoVendaPdfQuery(id), ct);
        return File(bytes, "application/pdf", $"PedidoVenda_{id}.pdf");
    }
}
