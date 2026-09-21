using ERP.Application.Financeiro.ContasPagar.Commands;
using ERP.Application.Financeiro.ContasPagar.Queries;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/contas-pagar")]
[Authorize]
public class ContasPagarController : ControllerBase
{
    private readonly IMediator _mediator;
    public ContasPagarController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? busca,
        [FromQuery] int? fornecedorId,
        [FromQuery] StatusContaFinanceira? status,
        [FromQuery] DateTime? dataVencimentoInicio,
        [FromQuery] DateTime? dataVencimentoFim,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetContasPagarQuery(busca, fornecedorId, status, dataVencimentoInicio, dataVencimentoFim, pagina, tamanho), ct);
        return Ok(result);
    }

    [HttpGet("resumo")]
    public async Task<IActionResult> GetResumo(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetResumoContasPagarQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetContaPagarByIdQuery(id), ct);
        return result.Sucesso ? Ok(result.Dados) : NotFound(result.Erros);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContaPagarCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.Sucesso ? Ok(result.Dados) : BadRequest(result.Erros);
    }

    [HttpPost("{id}/baixar")]
    public async Task<IActionResult> Baixar(int id, [FromBody] BaixarContaPagarCommand command, CancellationToken ct = default)
    {
        var cmd = command with { ContaPagarId = id };
        var result = await _mediator.Send(cmd, ct);
        return result.Sucesso ? Ok(result.Dados) : BadRequest(result.Erros);
    }

    [HttpPost("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new CancelarContaPagarCommand(id), ct);
        return result.Sucesso ? Ok() : BadRequest(result.Erros);
    }

    [HttpDelete("baixas/{baixaId}")]
    public async Task<IActionResult> EstornarBaixa(int baixaId, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new EstornarBaixaContaPagarCommand(baixaId), ct);
        return result.Sucesso ? NoContent() : BadRequest(result.Erros);
    }
}
