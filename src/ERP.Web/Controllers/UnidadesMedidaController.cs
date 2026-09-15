using ERP.Application.Cadastros.UnidadesMedida.Commands;
using ERP.Application.Cadastros.UnidadesMedida.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UnidadesMedidaController : ControllerBase
{
    private readonly IMediator _mediator;
    public UnidadesMedidaController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool apenasAtivas = true, CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetUnidadesMedidaQuery(apenasAtivas), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUnidadeMedidaCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.Sucesso ? Ok(result.Dados) : BadRequest(result.Erros);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUnidadeMedidaCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command with { Id = id }, ct);
        return result.Sucesso ? NoContent() : BadRequest(result.Erros);
    }
}
