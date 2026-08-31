namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Serviço para obter o usuário logado e contexto da requisição atual.
/// </summary>
public interface ICurrentUserService
{
    string? UsuarioId { get; }
    string? UserId => UsuarioId;
    string? UserName { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    int? EmpresaAtualId { get; }
    IEnumerable<string> Permissoes { get; }
}
