namespace ERP.Domain.Common;

/// <summary>
/// Interface para Unit of Work pattern.
/// Garante que todas as alterações de uma operação sejam salvas atomicamente.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
