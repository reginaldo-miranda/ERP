using ERP.Application.Estoque.Commands;
using ERP.Application.Estoque.DTOs;
using ERP.Application.Estoque.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/estoque")]
[Authorize]
public class EstoqueController : ControllerBase
{
    private readonly IMediator _mediator;

    public EstoqueController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("posicao")]
    public async Task<IActionResult> GetPosicao(
        [FromQuery] int? depositoId,
        [FromQuery] string? busca,
        [FromQuery] bool? somenteAbaixoMinimo,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetPosicaoEstoqueQuery(depositoId, busca, somenteAbaixoMinimo), ct);
        return Ok(result);
    }

    [HttpGet("movimentacoes")]
    public async Task<IActionResult> GetMovimentacoes(
        [FromQuery] int? produtoId,
        [FromQuery] int? depositoId,
        [FromQuery] int top = 100,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMovimentacoesEstoqueQuery(produtoId, depositoId, top), ct);
        return Ok(result);
    }

    [HttpPost("movimentacao")]
    public async Task<IActionResult> RegistrarMovimentacao([FromBody] RegistrarMovimentacaoDto dto, CancellationToken ct = default)
    {
        try
        {
            var id = await _mediator.Send(new RegistrarMovimentacaoEstoqueCommand(dto), ct);
            return Ok(new { Id = id, Mensagem = "Movimentação de estoque registrada com sucesso!" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Mensagem = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Mensagem = ex.Message });
        }
    }
}
