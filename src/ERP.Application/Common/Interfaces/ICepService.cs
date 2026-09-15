namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Serviço para busca de endereço por CEP (integração ViaCEP).
/// </summary>
public interface ICepService
{
    Task<EnderecoViaCepDto?> BuscarPorCepAsync(string cep, CancellationToken cancellationToken = default);
}

public class EnderecoViaCepDto
{
    public string Logradouro { get; set; } = string.Empty;
    public string Complemento { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Localidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public string Ibge { get; set; } = string.Empty;
    public bool Erro { get; set; }
}
