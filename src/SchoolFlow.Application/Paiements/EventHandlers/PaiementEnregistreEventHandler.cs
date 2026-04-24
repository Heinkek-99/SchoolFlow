using MediatR;
using Microsoft.Extensions.Logging;
using SchoolFlow.Application.Common.Events;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Paiements.EventHandlers;

public class PaiementEnregistreEventHandler
    : INotificationHandler<DomainEventNotification<PaiementEnregistreEvent>>
{
    private readonly ILogger<PaiementEnregistreEventHandler> _logger;

    public PaiementEnregistreEventHandler(ILogger<PaiementEnregistreEventHandler> logger)
        => _logger = logger;

    public Task Handle(
        DomainEventNotification<PaiementEnregistreEvent> notification,
        CancellationToken ct)
    {
        var ev = notification.DomainEvent;
        _logger.LogInformation(
            "Paiement enregistré — Id: {PaiementId}, Numéro: {Numero}, Famille: {FamilleId}, Montant: {Montant} FCFA, Ventilations: {NbVentilations}",
            ev.PaiementId, ev.NumeroPaiement, ev.FamilleId, ev.MontantTotal, ev.NombreVentilations);
        return Task.CompletedTask;
    }
}
