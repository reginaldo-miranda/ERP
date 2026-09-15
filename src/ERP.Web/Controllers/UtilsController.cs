using ERP.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UtilsController : ControllerBase
{
    private readonly ICepService _cepService;
    public UtilsController(ICepService cepService) => _cepService = cepService;

    /// <summary>
    /// Busca um endereço pelo CEP usando a API ViaCEP.
    /// </summary>
    [HttpGet("cep/{cep}")]
    public async Task<IActionResult> BuscarCep(string cep, CancellationToken ct = default)
    {
        var endereco = await _cepService.BuscarPorCepAsync(cep, ct);
        if (endereco == null) return NotFound(new { erro = "CEP não encontrado." });
        return Ok(endereco);
    }
}
