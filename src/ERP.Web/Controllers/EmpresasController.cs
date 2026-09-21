using ERP.Application.Empresas.Commands.AtualizarEmpresa;
using ERP.Application.Empresas.Commands.CriarEmpresa;
using ERP.Application.Empresas.Commands.DesativarEmpresa;
using ERP.Application.Empresas.Queries.GetEmpresaById;
using ERP.Application.Empresas.Queries.GetEmpresas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmpresasController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmpresasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool apenasAtivas = false, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetEmpresasQuery(apenasAtivas), ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetEmpresaByIdQuery(id), ct);
        return result != null ? Ok(result) : NotFound(new { erro = "Empresa não encontrada." });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CriarEmpresaCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.Sucesso ? Ok(result.Dados) : BadRequest(new { erros = result.Erros });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AtualizarEmpresaCommand command, CancellationToken ct = default)
    {
        if (id != command.Id)
            return BadRequest(new { erro = "O Id da rota difere do corpo da requisição." });

        var result = await _mediator.Send(command, ct);
        return result.Sucesso ? Ok(result.Dados) : BadRequest(new { erros = result.Erros });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new DesativarEmpresaCommand(id), ct);
        return result.Sucesso ? NoContent() : NotFound(new { erros = result.Erros });
    }
}
