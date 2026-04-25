using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolFlow.Application.Common.Events;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Eleves.EventHandlers;

public class EleveInscritEventHandler
    : INotificationHandler<DomainEventNotification<EleveInscritEvent>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<EleveInscritEventHandler> _logger;

    public EleveInscritEventHandler(
        IApplicationDbContext context,
        ILogger<EleveInscritEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<EleveInscritEvent> notification,
        CancellationToken ct)
    {
        var ev = notification.DomainEvent;
        _logger.LogInformation(
            "EleveInscrit — Id:{EleveId} Matricule:{Matricule} Classe:{ClasseId} École:{EcoleId}",
            ev.EleveId, ev.Matricule, ev.ClasseId, ev.EcoleId);

        await GenererFraisAutomatiquesAsync(
            ev.EleveId, ev.EcoleId, ev.ClasseId, ev.AnneeScolaireId, ct);
    }

    private async Task GenererFraisAutomatiquesAsync(
        Guid eleveId, Guid ecoleId, Guid classeId, Guid anneeScolaireId, CancellationToken ct)
    {
        var classe = await _context.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == classeId, ct);

        if (classe is null) return;

        var typesFraisAuto = await _context.TypeFrais
            .Where(t => t.EcoleId == ecoleId && t.GenerationAutomatique && !t.IsArchived)
            .ToListAsync(ct);

        if (!typesFraisAuto.Any()) return;

        var anneeScolaire = await _context.AnneeScolaires
            .Include(a => a.Periodes)
            .FirstOrDefaultAsync(a => a.Id == anneeScolaireId, ct);

        if (anneeScolaire is null) return;

        var fraisListe = new List<Frais>();

        foreach (var typeFrais in typesFraisAuto)
        {
            if (!typeFrais.MontantsParNiveau.TryGetValue(classe.Niveau, out var montant))
                continue;

            if (typeFrais.Categorie == CategorieFrais.Inscription)
            {
                fraisListe.Add(new Frais
                {
                    EcoleId = ecoleId,
                    EleveId = eleveId,
                    TypeFraisId = typeFrais.Id,
                    Montant = montant,
                    DateEcheance = DateTime.UtcNow.AddDays(30),
                    Remarques = "Frais d'inscription — généré automatiquement"
                });
            }
            else if (typeFrais.Categorie == CategorieFrais.Scolarite)
            {
                var trimestres = anneeScolaire.Periodes
                    .Where(p => p.Type == TypePeriode.Trimestre)
                    .OrderBy(p => p.Numero)
                    .ToList();

                if (trimestres.Any())
                {
                    var montantParTrimestre = Math.Round(montant / trimestres.Count, 0);
                    foreach (var trimestre in trimestres)
                    {
                        fraisListe.Add(new Frais
                        {
                            EcoleId = ecoleId,
                            EleveId = eleveId,
                            TypeFraisId = typeFrais.Id,
                            PeriodeId = trimestre.Id,
                            Montant = montantParTrimestre,
                            DateEcheance = trimestre.DateDebut.AddDays(15),
                            Remarques = $"Scolarité {trimestre.Libelle} — généré automatiquement"
                        });
                    }
                }
                else
                {
                    fraisListe.Add(new Frais
                    {
                        EcoleId = ecoleId,
                        EleveId = eleveId,
                        TypeFraisId = typeFrais.Id,
                        Montant = montant,
                        DateEcheance = DateTime.UtcNow.AddDays(60),
                        Remarques = "Scolarité annuelle — généré automatiquement"
                    });
                }
            }
        }

        if (fraisListe.Any())
        {
            _context.Frais.AddRange(fraisListe);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation(
                "{Count} frais générés pour élève {EleveId}",
                fraisListe.Count, eleveId);
        }
    }
}
