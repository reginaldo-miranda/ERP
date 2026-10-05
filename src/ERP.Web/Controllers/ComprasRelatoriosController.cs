using ERP.Application.Compras.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/compras")]
[Authorize]
public class ComprasRelatoriosController : ControllerBase
{
    private readonly IMediator _mediator;

    public ComprasRelatoriosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("historico")]
    public async Task<IActionResult> ObterHistorico(
        [FromQuery] int? fornecedorId,
        [FromQuery] int? produtoId,
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 50)
    {
        var result = await _mediator.Send(new GetHistoricoComprasQuery(
            fornecedorId, produtoId, dataInicio, dataFim, pagina, tamanhoPagina));
        return Ok(result);
    }

    [HttpGet("comparativo-precos/{produtoId:int}")]
    public async Task<IActionResult> ObterComparativoPrecos(int produtoId)
    {
        var result = await _mediator.Send(new GetComparativoPrecosFornecedoresQuery(produtoId));
        return Ok(result);
    }

    [HttpGet("resumo")]
    public async Task<IActionResult> ObterResumo([FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
    {
        var result = await _mediator.Send(new GetResumoComprasQuery(dataInicio, dataFim));
        return Ok(result);
    }
}
