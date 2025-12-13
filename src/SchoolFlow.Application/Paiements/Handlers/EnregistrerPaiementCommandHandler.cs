using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Paiements.Commands;
using SchoolFlow.Domain.Entities;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Paiements.Handlers;


public class EnregistrerPaiementCommandHandler : IRequestHandler<EnregistrerPaiementCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public EnregistrerPaiementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(EnregistrerPaiementCommand request, CancellationToken ct)
    {
        
        // 1. Vérifier que la famille existe
        var famille = await _context.Familles
            .Include(f => f.Eleves)
            .FirstOrDefaultAsync(f => f.Id == request.FamilleId, ct);

        if (famille == null)
            return Result<string>.Failure("Famille introuvable");

        // 2. Vérifier que tous les élèves appartiennent à cette famille
        var elevesIds = request.Ventilations.Select(v => v.EleveId).Distinct().ToList();
        var elevesInvalides = elevesIds.Except(famille.Eleves.Select(e => e.Id)).ToList();

        if (elevesInvalides.Any())
            return Result<string>.Failure("Certains élèves ne font pas partie de cette famille");

        // 3. Enregistrer le paiement
        var numeroPaiement = await GenerateNumeroPaiementAsync(ct);

        var paiement = new Paiement
        {
            Id = Guid.NewGuid(),
            NumeroPaiement = numeroPaiement,
            FamilleId = request.FamilleId,
            MontantTotal = request.MontantTotal,
            DatePaiement = request.DatePaiement,
            ModePaiement = request.ModePaiement,
            Reference = request.Reference,
            Commentaire = request.Commentaire,
            EnregistrePar = request.EnregistrePar,
            CreatedAt = DateTime.UtcNow
        };

        _context.Paiements.Add(paiement);

        // Créer ventilations
        foreach (var ventDto in request.Ventilations)
        {
             var resultatImputation = await ImputerPaiementSurEleve(
                paiement.Id,
                ventDto.EleveId,
                ventDto.Montant,
                ventDto.Remarque,
                ct
            );

            if (!resultatImputation.IsSuccess)
            {
                var errorMessage = resultatImputation.Error ?? "Erreur inconnue lors de l'imputation du paiement";
                return Result<string>.Failure(errorMessage);
            }
        }

        await _context.SaveChangesAsync(ct);

        return Result<string>.Success(numeroPaiement);
    }

    /// <summary>
    /// Impute un montant sur les frais impayés d'un élève (stratégie FIFO)
    /// </summary>
    private async Task<Result<bool>> ImputerPaiementSurEleve(
        Guid paiementId,
        Guid eleveId,
        decimal montant,
        string? remarque,
        CancellationToken ct)
    {
        // Récupérer tous les frais impayés de l'élève, triés par priorité
        var fraisImpayes = await _context.Frais
            .Where(f => f.EleveId == eleveId && !f.IsArchived)
            .OrderBy(f => f.DateEcheance)  // FIFO : plus ancien en premier
            .ThenBy(f => f.CreatedAt)
            .ToListAsync(ct);

        if (!fraisImpayes.Any())
        {
            return Result<bool>.Failure(
                $"Aucun frais trouvé pour l'élève. " +
                "Créez d'abord des frais avant d'enregistrer un paiement."
            );
        }

        var montantRestant = montant;
        var ventilationsCreees = 0;

        // Imputer le montant sur les frais (FIFO)
        foreach (var frais in fraisImpayes)
        {
            if (montantRestant <= 0.01m)
                break;

            var soldeRestantFrais = frais.Montant - frais.MontantPaye;

            if (soldeRestantFrais <= 0.01m)
                continue; // Frais déjà soldé, passer au suivant

            // Calculer le montant à imputer sur ce frais
            var montantAImputer = Math.Min(montantRestant, soldeRestantFrais);

            // ✅ TRAÇABILITÉ : Créer la ventilation avec FraisId renseigné
            var ventilation = new VentilationPaiement
            {
                Id = Guid.NewGuid(),
                PaiementId = paiementId,
                EleveId = eleveId,
                FraisId = frais.Id,  // ✅ On garde la trace du frais imputé
                MontantVentile = montantAImputer,
                Remarque = remarque ?? $"Imputation automatique sur {frais.TypeFrais.Libelle}",
                CreatedAt = DateTime.UtcNow
            };

            _context.VentilationsPaiement.Add(ventilation);

            // Mettre à jour le montant payé du frais
            frais.MontantPaye += montantAImputer;
            montantRestant -= montantAImputer;
            ventilationsCreees++;
        }

        // Vérifier si tout le montant a été imputé
        if (montantRestant > 0.01m)
        {
            var soldeTotalEleve = fraisImpayes.Sum(f => f.Montant - f.MontantPaye);
            return Result<bool>.Failure(
                $"Montant excédentaire de {montantRestant:N0} FCFA pour l'élève. " +
                $"Solde restant de l'élève : {soldeTotalEleve:N0} FCFA. " +
                "Réduisez le montant ou créez des frais supplémentaires."
            );
        }

        if (ventilationsCreees == 0)
        {
            return Result<bool>.Failure(
                "Aucune ventilation n'a été créée. Tous les frais de l'élève sont déjà payés."
            );
        }

        return Result<bool>.Success(true);
    }

    private async Task<string> GenerateNumeroPaiementAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var lastPaiement = await _context.Paiements
            .Where(p => p.NumeroPaiement.StartsWith($"PAY-{year}"))
            .OrderByDescending(p => p.NumeroPaiement)
            .Select(p => p.NumeroPaiement)
            .FirstOrDefaultAsync(ct);

        int sequence = 1;
        if (lastPaiement != null)
        {
            var lastSeq = lastPaiement.Split('-').Last();
            if (int.TryParse(lastSeq, out int num))
                sequence = num + 1;
        }

        return $"PAY-{year}-{sequence:D5}";
    }
}