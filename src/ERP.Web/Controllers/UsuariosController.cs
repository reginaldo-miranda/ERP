using ERP.Application.Usuarios.Commands.AlternarStatusUsuario;
using ERP.Application.Usuarios.Commands.AtualizarUsuario;
using ERP.Application.Usuarios.Commands.CriarUsuario;
using ERP.Application.Usuarios.Commands.ResetarSenhaUsuario;
using ERP.Application.Usuarios.Queries.GetPapeis;
using ERP.Application.Usuarios.Queries.GetUsuarios;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsuariosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetUsuariosQuery(), ct);
        return Ok(result);
    }

    [HttpGet("papeis")]
    public async Task<IActionResult> GetPapeis(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetPapeisQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CriarUsuarioCompletoCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.Sucesso ? Ok(new { id = result.Dados }) : BadRequest(new { erros = result.Erros });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] AtualizarUsuarioCommand command, CancellationToken ct = default)
    {
        if (id != command.Id)
            return BadRequest(new { erro = "O Id da rota difere do corpo da requisição." });

        var result = await _mediator.Send(command, ct);
        return result.Sucesso ? NoContent() : BadRequest(new { erros = result.Erros });
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ToggleStatus(string id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new AlternarStatusUsuarioCommand(id), ct);
        return result.Sucesso ? NoContent() : BadRequest(new { erros = result.Erros });
    }

    [HttpPost("{id}/resetar-senha")]
    public async Task<IActionResult> ResetPassword(string id, [FromBody] string novaSenha, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ResetarSenhaUsuarioCommand(id, novaSenha), ct);
        return result.Sucesso ? NoContent() : BadRequest(new { erros = result.Erros });
    }
}
