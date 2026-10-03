using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ERP.Application.Financeiro.Conciliacao.Commands;
using ERP.Application.Financeiro.Conciliacao.Queries;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/conciliacao")]
public class ConciliacaoController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConciliacaoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Importar extrato bancário OFX.
    /// </summary>
    [HttpPost("importar")]
    public async Task<IActionResult> Importar(
        [FromQuery] int contaBancariaId,
        IFormFile arquivo,
        CancellationToken ct = default)
    {
        if (arquivo == null || arquivo.Length == 0)
            return BadRequest("Nenhum arquivo enviado.");

        var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
        if (extensao != ".ofx")
            return BadRequest("Formato de arquivo inválido. Apenas arquivos .ofx são aceitos.");

        using var stream = arquivo.OpenReadStream();
        var result = await _mediator.Send(
            new ImportarExtratoOfxCommand(contaBancariaId, stream, arquivo.FileName), ct);

        if (!result.Sucesso)
            return BadRequest(result.Erros);

        return Ok(result.Dados);
    }

    /// <summary>
    /// Listar extratos importados.
    /// </summary>
    [HttpGet("extratos")]
    public async Task<IActionResult> ListarExtratos(
        [FromQuery] int? contaBancariaId,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetExtratosImportadosQuery(contaBancariaId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Listar itens de um extrato importado.
    /// </summary>
    [HttpGet("extratos/{extratoImportadoId:int}/itens")]
    public async Task<IActionResult> ListarItens(
        int extratoImportadoId,
        [FromQuery] StatusConciliacao? status,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetExtratoImportadoItensQuery(extratoImportadoId, status), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obter sugestões de conciliação para um extrato importado.
    /// </summary>
    [HttpGet("extratos/{extratoImportadoId:int}/sugestoes")]
    public async Task<IActionResult> ObterSugestoes(
        int extratoImportadoId,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetSugestoesConciliacaoQuery(extratoImportadoId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Conciliar um item do extrato com uma movimentação existente ou título.
    /// </summary>
    [HttpPost("conciliar")]
    public async Task<IActionResult> Conciliar(
        [FromBody] ConciliarItemCommand command,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.Sucesso)
            return BadRequest(result.Erros);
        return Ok(new { Sucesso = true });
    }

    /// <summary>
    /// Ignorar um item do extrato.
    /// </summary>
    [HttpPost("ignorar")]
    public async Task<IActionResult> Ignorar(
        [FromBody] IgnorarItemExtratoCommand command,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.Sucesso)
            return BadRequest(result.Erros);
        return Ok(new { Sucesso = true });
    }

    /// <summary>
    /// Desfazer conciliação de um item.
    /// </summary>
    [HttpPost("desfazer")]
    public async Task<IActionResult> Desfazer(
        [FromBody] DesfazerConciliacaoCommand command,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.Sucesso)
            return BadRequest(result.Erros);
        return Ok(new { Sucesso = true });
    }

    /// <summary>
    /// Criar movimentação a partir de item do extrato e auto-conciliar.
    /// </summary>
    [HttpPost("criar-movimentacao")]
    public async Task<IActionResult> CriarMovimentacao(
        [FromBody] CriarMovimentacaoDeExtratoCommand command,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.Sucesso)
            return BadRequest(result.Erros);
        return Ok(new { Sucesso = true });
    }
}
