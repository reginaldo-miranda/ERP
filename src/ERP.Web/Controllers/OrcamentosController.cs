using ERP.Application.Vendas.Commands;
using ERP.Application.Vendas.DTOs;
using ERP.Application.Vendas.Queries;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/orcamentos")]
[Authorize]
public class OrcamentosController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrcamentosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? busca,
        [FromQuery] int? clienteId,
        [FromQuery] StatusOrcamento? status,
        [FromQuery] bool? apenasExpirados,
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetOrcamentosQuery(
            busca, clienteId, status, apenasExpirados, dataInicio, dataFim, pagina, tamanho), ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetOrcamentoByIdQuery(id), ct);
        return result.Sucesso ? Ok(result.Dados) : NotFound(result.Erros);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CriarOrcamentoDto dto, CancellationToken ct = default)
    {
        var id = await _mediator.Send(new CriarOrcamentoCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AtualizarOrcamentoDto dto, CancellationToken ct = default)
    {
        var sucesso = await _mediator.Send(new AtualizarOrcamentoCommand(id, dto), ct);
        return sucesso ? NoContent() : BadRequest();
    }

    [HttpPost("{id}/rejeitar")]
    public async Task<IActionResult> Rejeitar(int id, [FromBody] string? motivo, CancellationToken ct = default)
    {
        var sucesso = await _mediator.Send(new RejeitarOrcamentoCommand(id, motivo), ct);
        return sucesso ? Ok() : BadRequest();
    }

    [HttpPost("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id, CancellationToken ct = default)
    {
        var sucesso = await _mediator.Send(new CancelarOrcamentoCommand(id), ct);
        return sucesso ? Ok() : BadRequest();
    }

    [HttpPost("{id}/converter")]
    public async Task<IActionResult> Converter(int id, CancellationToken ct = default)
    {
        var pedidoId = await _mediator.Send(new ConverterOrcamentoEmPedidoCommand(id), ct);
        return Ok(new { pedidoId });
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GerarPdf(int id, CancellationToken ct = default)
    {
        var bytes = await _mediator.Send(new GerarOrcamentoPdfQuery(id), ct);
        return File(bytes, "application/pdf", $"Orcamento_{id}.pdf");
    }
}
