// ============================================================
// FICHIER : src/SchoolFlow.Application/Common/Events/DomainEventNotification.cs
// ACTION  : Fichier à CRÉER (nouveau)
//
// POURQUOI CE FICHIER EXISTE :
//   - Le Domain ne peut pas dépendre de MediatR (Clean Architecture)
//   - Mais MediatR a besoin de INotification pour publier
//   - Solution : wrapper générique dans l'Application layer
//
// PATTERN : "Domain Event → Notification" (Jason Taylor Clean Architecture)
//
// FLUX :
//   Domain.IDomainEvent
//       ↓ wrappé par
//   Application.DomainEventNotification<T> : INotification
//       ↓ publié par
//   Infrastructure.ApplicationDbContext via IPublisher
// ============================================================

using MediatR;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Common.Events;

/// <summary>
/// Wrapper MediatR pour un domain event.
/// Permet de publier n'importe quel IDomainEvent via MediatR.IPublisher
/// sans que le Domain sache que MediatR existe.
/// </summary>
public class DomainEventNotification<T> : INotification where T : IDomainEvent
{
    public T DomainEvent { get; }

    public DomainEventNotification(T domainEvent)
        => DomainEvent = domainEvent;
}