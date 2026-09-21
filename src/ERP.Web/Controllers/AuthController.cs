using ERP.Application.Auth.Commands.Login;
using ERP.Application.Auth.Commands.TrocarEmpresa;
using ERP.Application.Auth.Queries.ObterUsuarioAtual;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Realiza a autenticação do usuário retornando o JWT Token e dados da empresa.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.Sucesso ? Ok(result.Dados) : Unauthorized(new { erros = result.Erros });
    }

    /// <summary>
    /// Alterna a empresa ativa para o usuário autenticado.
    /// </summary>
    [HttpPost("trocar-empresa")]
    [Authorize]
    public async Task<IActionResult> TrocarEmpresa([FromBody] TrocarEmpresaCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.Sucesso ? Ok(result.Dados) : BadRequest(new { erros = result.Erros });
    }

    /// <summary>
    /// Retorna os dados do usuário autenticado atual.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> ObterUsuarioAtual(CancellationToken ct = default)
    {
        var usuario = await _mediator.Send(new ObterUsuarioAtualQuery(), ct);
        return usuario != null ? Ok(usuario) : NotFound(new { erro = "Usuário não encontrado." });
    }
}
