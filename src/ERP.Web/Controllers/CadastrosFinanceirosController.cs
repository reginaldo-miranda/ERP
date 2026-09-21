using ERP.Application.Financeiro.Cadastros.Commands;
using ERP.Application.Financeiro.Cadastros.Queries;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/financeiro/cadastros")]
[Authorize]
public class CadastrosFinanceirosController : ControllerBase
{
    private readonly IMediator _mediator;
    public CadastrosFinanceirosController(IMediator mediator) => _mediator = mediator;

    [HttpGet("bancos")]
    public async Task<IActionResult> GetBancos([FromQuery] bool somenteAtivos = true, CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetBancosQuery(somenteAtivos), ct));

    [HttpGet("contas-bancarias")]
    public async Task<IActionResult> GetContasBancarias([FromQuery] bool somenteAtivas = true, CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetContasBancariasQuery(somenteAtivas), ct));

    [HttpPost("contas-bancarias")]
    public async Task<IActionResult> CreateContaBancaria([FromBody] CreateContaBancariaCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.Sucesso ? Ok(result.Dados) : BadRequest(result.Erros);
    }

    [HttpGet("formas-pagamento")]
    public async Task<IActionResult> GetFormasPagamento([FromQuery] bool somenteAtivas = true, CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetFormasPagamentoQuery(somenteAtivas), ct));

    [HttpPost("formas-pagamento")]
    public async Task<IActionResult> CreateFormaPagamento([FromBody] CreateFormaPagamentoCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.Sucesso ? Ok(result.Dados) : BadRequest(result.Erros);
    }

    [HttpGet("planos-contas")]
    public async Task<IActionResult> GetPlanosContas([FromQuery] TipoPlanoConta? tipo, [FromQuery] bool? somenteAnaliticas, [FromQuery] bool somenteAtivos = true, CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetPlanosContasQuery(tipo, somenteAnaliticas, somenteAtivos), ct));

    [HttpPost("planos-contas")]
    public async Task<IActionResult> CreatePlanoConta([FromBody] CreatePlanoContaCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.Sucesso ? Ok(result.Dados) : BadRequest(result.Erros);
    }

    [HttpGet("centros-custo")]
    public async Task<IActionResult> GetCentrosCusto([FromQuery] bool somenteAtivos = true, CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetCentrosCustoQuery(somenteAtivos), ct));

    [HttpPost("centros-custo")]
    public async Task<IActionResult> CreateCentroCusto([FromBody] CreateCentroCustoCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.Sucesso ? Ok(result.Dados) : BadRequest(result.Erros);
    }

    [HttpGet("extrato-bancario")]
    public async Task<IActionResult> GetExtratoBancario(
        [FromQuery] int contaBancariaId,
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        [FromQuery] TipoOperacaoFinanceira? tipo = null,
        [FromQuery] int? planoContaId = null,
        [FromQuery] int? centroCustoId = null,
        CancellationToken ct = default)
        => Ok(await _mediator.Send(
            new GetExtratoBancarioQuery(contaBancariaId, dataInicio, dataFim, tipo, planoContaId, centroCustoId), ct));
}
