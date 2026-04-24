using MediatR;
using Microsoft.Extensions.Logging;
using SchoolFlow.Application.Common.Events;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Eleves.EventHandlers;

public class EleveInscritEventHandler
    : INotificationHandler<DomainEventNotification<EleveInscritEvent>>
{
    private readonly ILogger<EleveInscritEventHandler> _logger;

    public EleveInscritEventHandler(ILogger<EleveInscritEventHandler> logger)
        => _logger = logger;

    public Task Handle(
        DomainEventNotification<EleveInscritEvent> notification,
        CancellationToken ct)
    {
        var ev = notification.DomainEvent;
        _logger.LogInformation(
            "Élève inscrit — Id: {EleveId}, Matricule: {Matricule}, Classe: {ClasseId}, École: {EcoleId}",
            ev.EleveId, ev.Matricule, ev.ClasseId, ev.EcoleId);
        return Task.CompletedTask;
    }
}
