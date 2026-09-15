using System.Text.Json;
using System.Text.Json.Serialization;
using ERP.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Implementação do ICepService usando a API pública ViaCEP (https://viacep.com.br).
/// </summary>
public class ViaCepService : ICepService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ViaCepService> _logger;

    public ViaCepService(HttpClient httpClient, ILogger<ViaCepService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<EnderecoViaCepDto?> BuscarPorCepAsync(string cep, CancellationToken cancellationToken = default)
    {
        var cepLimpo = new string(cep.Where(char.IsDigit).ToArray());
        if (cepLimpo.Length != 8) return null;

        try
        {
            var url = $"https://viacep.com.br/ws/{cepLimpo}/json/";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var resultado = JsonSerializer.Deserialize<ViaCepResponse>(content, options);

            if (resultado?.Erro == true) return null;

            return new EnderecoViaCepDto
            {
                Logradouro = resultado?.Logradouro ?? string.Empty,
                Complemento = resultado?.Complemento ?? string.Empty,
                Bairro = resultado?.Bairro ?? string.Empty,
                Localidade = resultado?.Localidade ?? string.Empty,
                Uf = resultado?.Uf ?? string.Empty,
                Ibge = resultado?.Ibge ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao consultar CEP {Cep} no ViaCEP.", cep);
            return null;
        }
    }

    private class ViaCepResponse
    {
        public string? Logradouro { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public string? Localidade { get; set; }
        public string? Uf { get; set; }
        public string? Ibge { get; set; }

        [JsonPropertyName("erro")]
        public bool? Erro { get; set; }
    }
}
