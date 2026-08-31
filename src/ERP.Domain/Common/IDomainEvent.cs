using MediatR;

namespace ERP.Domain.Common;

/// <summary>
/// Interface marker para Domain Events.
/// Domain Events são usados para comunicação desacoplada entre módulos.
/// Implementa INotification do MediatR para suporte a publish/subscribe.
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}

/// <summary>
/// Classe base para Domain Events com timestamp automático.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
