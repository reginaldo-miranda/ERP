namespace ERP.Domain.Core.Interfaces;

/// <summary>
/// Abstração para operações de Identity (login, registro, etc.).
/// Implementada pela camada Infrastructure usando ASP.NET Identity.
/// </summary>
public interface IIdentityService
{
    Task<(bool Sucesso, string? UsuarioId, string[] Erros)> CriarUsuarioAsync(string email, string senha, string nomeCompleto);
    Task<(bool Sucesso, string? UsuarioId)> ValidarCredenciaisAsync(string email, string senha);
    Task<bool> UsuarioExisteAsync(string email);
    Task<bool> AdicionarAoPapelAsync(string usuarioId, string papel);
    Task<bool> RemoverDoPapelAsync(string usuarioId, string papel);
    Task<IList<string>> ObterPapeisAsync(string usuarioId);
    Task<bool> AlterarSenhaAsync(string usuarioId, string senhaAtual, string novaSenha);
    Task<(string? NomeCompleto, string? Email)> ObterDadosUsuarioAsync(string usuarioId);
}
