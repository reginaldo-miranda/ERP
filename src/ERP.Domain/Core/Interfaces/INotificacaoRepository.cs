using ERP.Domain.Core.Entities;

namespace ERP.Domain.Core.Interfaces;

/// <summary>
/// Interface de repositório para Notificações.
/// </summary>
public interface INotificacaoRepository : Common.IRepository<Notificacao>
{
    Task<IReadOnlyList<Notificacao>> ObterPorUsuarioAsync(string usuarioId, bool? apenasNaoLidas = null, CancellationToken cancellationToken = default);
    Task<int> ContarNaoLidasAsync(string usuarioId, CancellationToken cancellationToken = default);
    Task MarcarComoLidaAsync(int id, CancellationToken cancellationToken = default);
    Task MarcarTodasComoLidasAsync(string usuarioId, CancellationToken cancellationToken = default);
}
