using ERP.Application.Fiscal.Commands;
using ERP.Application.Fiscal.DTOs;
using ERP.Application.Fiscal.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/fiscal/certificado")]
[Authorize]
public class CertificadosController : ControllerBase
{
    private readonly IMediator _mediator;

    public CertificadosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> ObterAtivo(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetCertificadoDigitalQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile arquivo,
        [FromForm] string senha,
        CancellationToken ct = default)
    {
        if (arquivo == null || arquivo.Length == 0)
            return BadRequest("Arquivo do certificado digital (.pfx ou .p12) não foi enviado.");

        if (string.IsNullOrWhiteSpace(senha))
            return BadRequest("Senha do certificado digital é obrigatória.");

        byte[] bytes;
        using (var ms = new MemoryStream())
        {
            await arquivo.CopyToAsync(ms, ct);
            bytes = ms.ToArray();
        }

        try
        {
            var dto = new SalvarCertificadoDigitalDto
            {
                NomeArquivo = arquivo.FileName,
                ArquivoBytes = bytes,
                Senha = senha
            };

            var result = await _mediator.Send(new SalvarCertificadoCommand(dto), ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Mensagem = ex.Message });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Remover(CancellationToken ct = default)
    {
        await _mediator.Send(new RemoverCertificadoCommand(), ct);
        return NoContent();
    }
}
