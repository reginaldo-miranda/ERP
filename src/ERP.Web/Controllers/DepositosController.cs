using ERP.Application.Estoque.Commands;
using ERP.Application.Estoque.DTOs;
using ERP.Application.Estoque.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/depositos")]
[Authorize]
public class DepositosController : ControllerBase
{
    private readonly IMediator _mediator;

    public DepositosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool somenteAtivos = true, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetDepositosQuery(somenteAtivos), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CriarDepositoDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
            return BadRequest(new { Mensagem = "O nome do depósito é obrigatório." });

        var id = await _mediator.Send(new CriarDepositoCommand(dto), ct);
        return Ok(new { Id = id, Mensagem = "Depósito criado com sucesso!" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CriarDepositoDto dto, CancellationToken ct = default)
    {
        var sucesso = await _mediator.Send(new AtualizarDepositoCommand(id, dto), ct);
        if (!sucesso)
            return NotFound(new { Mensagem = "Depósito não encontrado." });

        return Ok(new { Mensagem = "Depósito atualizado com sucesso!" });
    }
}
