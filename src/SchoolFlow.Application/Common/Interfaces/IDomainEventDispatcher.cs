using SchoolFlow.Domain.Entities;
 
namespace SchoolFlow.Application.Common.Interfaces;
 
/// <summary>
/// Dispatche les domain events vers MediatR.
/// Défini dans Application, implémenté dans Infrastructure.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default);
}
 