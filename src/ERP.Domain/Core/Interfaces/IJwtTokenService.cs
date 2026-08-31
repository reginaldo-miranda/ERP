namespace ERP.Domain.Core.Interfaces;

/// <summary>
/// Abstração para geração e validação de tokens JWT.
/// Implementada pela camada Infrastructure.
/// </summary>
public interface IJwtTokenService
{
    string GerarToken(string usuarioId, string email, string nomeCompleto, int empresaAtualId, IEnumerable<int> empresaIds, IEnumerable<string> permissoes);
    string GerarRefreshToken();
    (bool Valido, string? UsuarioId) ValidarToken(string token);
}
