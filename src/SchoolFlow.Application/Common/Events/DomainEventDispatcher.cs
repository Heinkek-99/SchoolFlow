namespace SchoolFlow.Infrastructure.Events;
 
using MediatR;
using SchoolFlow.Application.Common.Events;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Domain.Entities;
 
/// <summary>
/// Implémentation du dispatcher.
/// Wraps chaque IDomainEvent dans DomainEventNotification{T} puis publie via MediatR.
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublisher _publisher;
 
    public DomainEventDispatcher(IPublisher publisher)
        => _publisher = publisher;
 
    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken ct = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            // Crée DomainEventNotification<T> dynamiquement via réflexion
            var notificationType = typeof(DomainEventNotification<>)
                .MakeGenericType(domainEvent.GetType());
 
            var notification = Activator.CreateInstance(notificationType, domainEvent)
                as INotification;
 
            if (notification is not null)
                await _publisher.Publish(notification, ct);
        }
    }
}
 