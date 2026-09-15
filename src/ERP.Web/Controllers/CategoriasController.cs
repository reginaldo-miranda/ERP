using ERP.Application.Cadastros.Categorias.Commands;
using ERP.Application.Cadastros.Categorias.Queries;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly IMediator _mediator;
    public CategoriasController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TipoCategoria? tipo, [FromQuery] bool apenasAtivas = true, CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetCategoriasQuery(tipo, apenasAtivas), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoriaCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.Sucesso ? Ok(result.Dados) : BadRequest(result.Erros);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoriaCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command with { Id = id }, ct);
        return result.Sucesso ? NoContent() : BadRequest(result.Erros);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new DeleteCategoriaCommand(id), ct);
        return result.Sucesso ? NoContent() : NotFound(result.Erros);
    }
}
