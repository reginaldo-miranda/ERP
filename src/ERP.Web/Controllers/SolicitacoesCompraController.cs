using ERP.Application.Compras.Commands;
using ERP.Application.Compras.DTOs;
using ERP.Application.Compras.Queries;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/solicitacoes-compra")]
[Authorize]
public class SolicitacoesCompraController : ControllerBase
{
    private readonly IMediator _mediator;

    public SolicitacoesCompraController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca,
        [FromQuery] StatusSolicitacaoCompra? status,
        [FromQuery] PrioridadeSolicitacaoCompra? prioridade,
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 50)
    {
        var result = await _mediator.Send(new GetSolicitacoesCompraQuery(
            busca, status, prioridade, dataInicio, dataFim, pagina, tamanhoPagina));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var result = await _mediator.Send(new GetSolicitacaoCompraByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarSolicitacaoCompraDto dto)
    {
        var id = await _mediator.Send(new CriarSolicitacaoCompraCommand(dto));
        return CreatedAtAction(nameof(ObterPorId), new { id }, new { id });
    }

    [HttpPost("{id:int}/aprovar")]
    public async Task<IActionResult> Aprovar(int id, [FromBody] string aprovadorNome)
    {
        var sucesso = await _mediator.Send(new AprovarSolicitacaoCompraCommand(id, aprovadorNome));
        return Ok(new { sucesso });
    }

    [HttpPost("{id:int}/rejeitar")]
    public async Task<IActionResult> Rejeitar(int id, [FromBody] string motivo)
    {
        var sucesso = await _mediator.Send(new RejeitarSolicitacaoCompraCommand(id, motivo));
        return Ok(new { sucesso });
    }

    [HttpPost("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        var sucesso = await _mediator.Send(new CancelarSolicitacaoCompraCommand(id));
        return Ok(new { sucesso });
    }

    [HttpPost("{id:int}/converter")]
    public async Task<IActionResult> Converter(int id, [FromQuery] int fornecedorId, [FromQuery] int depositoDestinoId)
    {
        var pedidoId = await _mediator.Send(new ConverterSolicitacaoEmPedidoCommand(id, fornecedorId, depositoDestinoId));
        return Ok(new { pedidoId });
    }
}
