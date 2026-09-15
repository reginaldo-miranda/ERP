using ERP.Application.Cadastros.Fornecedores.Commands;
using ERP.Application.Cadastros.Fornecedores.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FornecedoresController : ControllerBase
{
    private readonly IMediator _mediator;
    public FornecedoresController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? busca, [FromQuery] bool? ativo, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 20, CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetFornecedoresQuery(busca, ativo, pagina, tamanho), ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetFornecedorByIdQuery(id), ct);
        return result.Sucesso ? Ok(result.Dados) : NotFound(result.Erros);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFornecedorCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.Sucesso ? Ok(result.Dados) : BadRequest(result.Erros);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFornecedorCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command with { Id = id }, ct);
        return result.Sucesso ? NoContent() : BadRequest(result.Erros);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new DeleteFornecedorCommand(id), ct);
        return result.Sucesso ? NoContent() : NotFound(result.Erros);
    }
}
