using ERP.Application.Estoque.Commands;
using ERP.Application.Estoque.DTOs;
using ERP.Application.Estoque.Queries;
using ERP.Domain.Core.Enums;
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

    [HttpPost("transferencia")]
    public async Task<IActionResult> TransferirEstoque([FromBody] TransferirEstoqueDto dto, CancellationToken ct = default)
    {
        try
        {
            await _mediator.Send(new TransferirEstoqueCommand(dto), ct);
            return Ok(new { Mensagem = "Transferência de estoque realizada com sucesso!" });
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

    [HttpGet("lotes")]
    public async Task<IActionResult> GetLotes(
        [FromQuery] int? produtoId,
        [FromQuery] int? depositoId,
        [FromQuery] bool? somenteAtivos = true,
        [FromQuery] bool? somenteVencidos = null,
        [FromQuery] bool? somenteCriticos30Dias = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetLotesQuery(produtoId, depositoId, somenteAtivos, somenteVencidos, somenteCriticos30Dias), ct);
        return Ok(result);
    }

    [HttpPost("lotes")]
    public async Task<IActionResult> CriarLote([FromBody] CriarLoteDto dto, CancellationToken ct = default)
    {
        try
        {
            var id = await _mediator.Send(new CriarLoteCommand(dto), ct);
            return Ok(new { Id = id, Mensagem = "Lote cadastrado com sucesso!" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Mensagem = ex.Message });
        }
    }

    [HttpGet("numeros-serie")]
    public async Task<IActionResult> GetNumerosSerie(
        [FromQuery] int? produtoId,
        [FromQuery] int? depositoId,
        [FromQuery] StatusNumeroSerie? status,
        [FromQuery] string? busca,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetNumerosSerieQuery(produtoId, depositoId, status, busca), ct);
        return Ok(result);
    }

    [HttpPost("numeros-serie")]
    public async Task<IActionResult> CadastrarNumeroSerie([FromBody] CadastrarNumeroSerieDto dto, CancellationToken ct = default)
    {
        try
        {
            var id = await _mediator.Send(new CadastrarNumeroSerieCommand(dto), ct);
            return Ok(new { Id = id, Mensagem = "Número de série registrado com sucesso!" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Mensagem = ex.Message });
        }
    }

    [HttpGet("inventarios")]
    public async Task<IActionResult> GetInventarios(
        [FromQuery] int? depositoId,
        [FromQuery] StatusInventario? status,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetInventariosQuery(depositoId, status), ct);
        return Ok(result);
    }

    [HttpGet("inventarios/{id}")]
    public async Task<IActionResult> GetInventarioPorId(int id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetInventarioPorIdQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("inventarios/{id}/itens")]
    public async Task<IActionResult> GetInventarioItens(int id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetInventarioItensQuery(id), ct);
        return Ok(result);
    }

    [HttpPost("inventarios")]
    public async Task<IActionResult> IniciarInventario([FromBody] IniciarInventarioDto dto, CancellationToken ct = default)
    {
        try
        {
            var id = await _mediator.Send(new IniciarInventarioCommand(dto), ct);
            return Ok(new { Id = id, Mensagem = "Inventário físico iniciado com sucesso!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Mensagem = ex.Message });
        }
    }

    [HttpPut("inventarios/{id}/contagem")]
    public async Task<IActionResult> AtualizarContagem(int id, [FromBody] List<AtualizarContagemItemDto> itens, CancellationToken ct = default)
    {
        try
        {
            await _mediator.Send(new AtualizarContagemInventarioCommand(id, itens), ct);
            return Ok(new { Mensagem = "Contagem salva com sucesso!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Mensagem = ex.Message });
        }
    }

    [HttpPost("inventarios/{id}/finalizar")]
    public async Task<IActionResult> FinalizarInventario(int id, [FromBody] FinalizarInventarioDto dto, CancellationToken ct = default)
    {
        try
        {
            var commandDto = dto with { InventarioId = id };
            await _mediator.Send(new FinalizarInventarioCommand(commandDto), ct);
            return Ok(new { Mensagem = "Inventário finalizado e ajustes de estoque aplicados com sucesso!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Mensagem = ex.Message });
        }
    }

    [HttpPost("inventarios/{id}/cancelar")]
    public async Task<IActionResult> CancelarInventario(int id, CancellationToken ct = default)
    {
        try
        {
            await _mediator.Send(new CancelarInventarioCommand(id), ct);
            return Ok(new { Mensagem = "Inventário cancelado." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Mensagem = ex.Message });
        }
    }
}
