using ERP.Application.Cadastros.Servicos.Commands;
using ERP.Application.Cadastros.Servicos.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicosController : ControllerBase
{
    private readonly IMediator _mediator;
    public ServicosController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? busca, [FromQuery] bool? ativo, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 20, CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetServicosQuery(busca, ativo, pagina, tamanho), ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetServicoByIdQuery(id), ct);
        return result.Sucesso ? Ok(result.Dados) : NotFound(result.Erros);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServicoCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.Sucesso ? Ok(result.Dados) : BadRequest(result.Erros);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServicoCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command with { Id = id }, ct);
        return result.Sucesso ? NoContent() : BadRequest(result.Erros);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new DeleteServicoCommand(id), ct);
        return result.Sucesso ? NoContent() : NotFound(result.Erros);
    }
}
