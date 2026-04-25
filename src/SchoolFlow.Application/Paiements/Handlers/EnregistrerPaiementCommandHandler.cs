using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Paiements.Commands;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Paiements.Handlers;

public class EnregistrerPaiementCommandHandler
    : IRequestHandler<EnregistrerPaiementCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public EnregistrerPaiementCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(
        EnregistrerPaiementCommand request,
        CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        if (ecoleId == Guid.Empty)
            return Result<string>.Failure("Contexte école manquant — reconnectez-vous.");

        // ── VALIDATIONS ────────────────────────────────────────────────────
        if (request.DatePaiement.Date > DateTime.UtcNow.Date)
            return Result<string>.Failure("La date de paiement ne peut pas être dans le futur.");

        if (request.MontantTotal <= 0)
            return Result<string>.Failure("Le montant doit être positif.");

        if (!request.Ventilations.Any())
            return Result<string>.Failure("Au moins une ventilation est requise.");

        var totalVentile = request.Ventilations.Sum(v => v.Montant);
        if (Math.Abs(totalVentile - request.MontantTotal) > 0.01m)
            return Result<string>.Failure(
                $"Somme des ventilations ({totalVentile:N0}) ≠ montant total ({request.MontantTotal:N0}) FCFA.");

        // ── ÉTAPE 1 : CHARGER ──────────────────────────────────────────────
        var famille = await _context.Familles
            .Include(f => f.Eleves)
            .FirstOrDefaultAsync(f => f.Id == request.FamilleId
                                   && f.EcoleId == ecoleId, ct);

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
        var numeroPaiement = await GenererNumeroPaiementAsync(ecoleId, ct);

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

        // ApplicationDbContext.SaveChangesAsync dispatche et efface les domain events.

        return Result<string>.Success(numeroPaiement);
    }

    private async Task<string> GenererNumeroPaiementAsync(Guid ecoleId, CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"PAY-{year}";

        var lastNum = await _context.Paiements
            .Where(p => p.EcoleId == ecoleId && p.NumeroPaiement.StartsWith(prefix))
            .OrderByDescending(p => p.NumeroPaiement)
            .Select(p => p.NumeroPaiement)
            .FirstOrDefaultAsync(ct);

        var sequence = 1;
        if (lastNum is not null)
        {
            var parts = lastNum.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var num))
                sequence = num + 1;
        }

        return $"{prefix}-{sequence:D5}";
    }
}