using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Paiements.Commands;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Paiements.Handlers;

/// <summary>
/// Handler APRÈS refactoring.
/// 
/// AVANT (problème) :
///   → ImputerPaiementSurEleve() avec 50 lignes de logique FIFO dans le Handler
///   → Le Handler "pense" — il décide comment imputer
///
/// APRÈS (correct) :
///   → Le Handler orchestre : Charger → Appeler Domain → Sauvegarder
///   → La logique FIFO est dans Paiement.AppliquerVentilation()
///   → Les Domain Events sont levés dans le Domain, publiés ici après Save
///
/// Pattern : Load → Act → Save → Publish Events
/// </summary>
public class EnregistrerPaiementCommandHandler 
    : IRequestHandler<EnregistrerPaiementCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher; // MediatR IPublisher pour les domain events
    private readonly ICurrentUserService _currentUser;

    public EnregistrerPaiementCommandHandler(
        IApplicationDbContext context,
        IPublisher publisher,
        ICurrentUserService currentUser)
    {
        _context = context;
        _publisher = publisher;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(
        EnregistrerPaiementCommand request, 
        CancellationToken ct)
    {
        // ── ÉTAPE 1 : CHARGER ──────────────────────────────────────────────
        var famille = await _context.Familles
            .Include(f => f.Eleves)
            .FirstOrDefaultAsync(f => f.Id == request.FamilleId 
                                   && f.EcoleId == _currentUser.EcoleId, ct);

        if (famille is null)
            return Result<string>.Failure("Famille introuvable.");

        // Vérifier appartenance des élèves
        var elevesIds = request.Ventilations.Select(v => v.EleveId).Distinct().ToList();
        var elevesInvalides = elevesIds.Except(famille.Eleves.Select(e => e.Id)).ToList();

        if (elevesInvalides.Count != 0)
            return Result<string>.Failure("Certains élèves ne font pas partie de cette famille.");

        // Charger les frais pour chaque élève concerné
        var tousLesEleves = await _context.Eleves
            .Include(e => e.Frais)
                .ThenInclude(f => f.TypeFrais)
            .Where(e => elevesIds.Contains(e.Id))
            .ToListAsync(ct);

        // ── ÉTAPE 2 : AGIR (le Domain fait le travail) ──────────────────────
        var numeroPaiement = await GenererNumeroPaiementAsync(ct);

        var paiement = Paiement.Creer(
            ecoleId: _currentUser.EcoleId,
            numeroPaiement: numeroPaiement,
            familleId: request.FamilleId,
            montantTotal: request.MontantTotal,
            datePaiement: request.DatePaiement,
            modePaiement: request.ModePaiement,
            enregistrePar: _currentUser.UserId ?? Guid.Empty,
            reference: request.Reference,
            commentaire: request.Commentaire
        );

        _context.Paiements.Add(paiement);

        // Déléguer la ventilation au Domain
        try
        {
            foreach (var ventDto in request.Ventilations)
            {
                var eleve = tousLesEleves.First(e => e.Id == ventDto.EleveId);
                var fraisOrdonnes = eleve.Frais
                    .Where(f => !f.IsArchived)
                    .OrderBy(f => f.DateEcheance)
                    .ThenBy(f => f.CreatedAt);

                // 🎯 TOUTE la logique FIFO est dans le Domain
                paiement.AppliquerVentilation(ventDto.EleveId, ventDto.Montant, fraisOrdonnes);
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result<string>.Failure(ex.Message);
        }

        // ── ÉTAPE 3 : SAUVEGARDER ───────────────────────────────────────────
        await _context.SaveChangesAsync(ct);

        // ── ÉTAPE 4 : PUBLIER LES DOMAIN EVENTS ────────────────────────────
        // Les events ont été collectés dans le domain pendant l'Act.
        // On les publie ICI, après le commit, pour garantir la cohérence.
        foreach (var domainEvent in paiement.DomainEvents)
            await _publisher.Publish(domainEvent, ct);

        paiement.ClearDomainEvents();

        return Result<string>.Success(numeroPaiement);
    }

    private async Task<string> GenererNumeroPaiementAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var lastNum = await _context.Paiements
            .Where(p => p.NumeroPaiement.StartsWith($"PAY-{year}"))
            .OrderByDescending(p => p.NumeroPaiement)
            .Select(p => p.NumeroPaiement)
            .FirstOrDefaultAsync(ct);

        var sequence = 1;
        if (lastNum is not null)
        {
            var lastSeq = lastNum.Split('-').Last();
            if (int.TryParse(lastSeq, out var num)) sequence = num + 1;
        }

        return $"PAY-{year}-{sequence:D5}";
    }
}