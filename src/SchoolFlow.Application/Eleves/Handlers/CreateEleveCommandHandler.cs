using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Eleves.Commands;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Eleves.Handlers;

public class CreateEleveCommandHandler
    : IRequestHandler<CreateEleveCommand, Result<CreateEleveResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateEleveCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<CreateEleveResponse>> Handle(
        CreateEleveCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;  // ← MULTI-TENANT

        // ── 1. VÉRIFICATIONS ────────────────────────────────────────────────
        var famille = await _context.Familles
            .FirstOrDefaultAsync(f => f.Id == request.FamilleId
                                   && f.EcoleId == ecoleId, ct);

        if (famille is null)
            return Result<CreateEleveResponse>.Failure("Famille introuvable.");

        var classe = await _context.Classes
            .Include(c => c.AnneeScolaire)
            .FirstOrDefaultAsync(c => c.Id == request.ClasseId
                                   && c.EcoleId == ecoleId, ct);

        if (classe is null)
            return Result<CreateEleveResponse>.Failure("Classe introuvable.");

        // ── 2. GÉNÉRER MATRICULE ────────────────────────────────────────────
        var matricule = await GenererMatriculeAsync(ecoleId, ct);

        // ── 3. CRÉER L'ÉLÈVE (+ raise EleveInscritEvent) ────────────────────
        var eleve = Eleve.Inscrire(
            nom: request.Nom,
            prenom: request.Prenom,
            dateNaissance: request.DateNaissance,
            lieuNaissance: request.LieuNaissance,
            sexe: request.Sexe,
            familleId: request.FamilleId,
            classeId: request.ClasseId,
            anneeScolaireId: classe.AnneeScolaireId,
            ecoleId: ecoleId,
            matricule: matricule,
            photoPath: request.PhotoPath,
            nationalite: request.Nationalite,
            groupeSanguin: request.GroupeSanguin,
            allergies: request.Allergies,
            contactUrgence: request.ContactUrgence);

        _context.Eleves.Add(eleve);

        // ── 4. SAUVEGARDER ──────────────────────────────────────────────────
        await _context.SaveChangesAsync(ct);

        // ── 5. GÉNÉRER LES FRAIS AUTOMATIQUES ──────────────────────────────
        // Fait APRÈS le premier save pour avoir l'EleveId
        await GenererFraisAutomatiquesAsync(eleve.Id, ecoleId, classe.Niveau, classe.AnneeScolaireId, ct);

        return Result<CreateEleveResponse>.Success(
            new CreateEleveResponse(eleve.Id, matricule));
    }

    // ─── HELPERS ────────────────────────────────────────────────────────────

    private async Task<string> GenererMatriculeAsync(Guid ecoleId, CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;

        // Préfixe école pour unicité inter-établissement
        var prefix = $"EL{year}";
        int sequence = 1;

        var lastMatricule = await _context.Eleves
            .Where(e => e.EcoleId == ecoleId && e.Matricule.StartsWith(prefix))
            .OrderByDescending(e => e.Matricule)
            .Select(e => e.Matricule)
            .FirstOrDefaultAsync(ct);

        if (lastMatricule is not null)
        {
            var parts = lastMatricule.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[1], out var num))
                sequence = num + 1;
        }

        string newMatricule;
        do
        {
            newMatricule = $"{prefix}-{sequence:D5}";
            sequence++;
        } while (await _context.Eleves.AnyAsync(e => e.Matricule == newMatricule, ct));

        return newMatricule;
    }

    private async Task GenererFraisAutomatiquesAsync(
        Guid eleveId,
        Guid ecoleId,
        Niveau niveau,
        Guid anneeScolaireId,
        CancellationToken ct)
    {
        // Charger les types de frais auto de CETTE école uniquement
        var typesFraisAuto = await _context.TypeFrais
            .Where(t => t.EcoleId == ecoleId      // ← MULTI-TENANT
                     && t.GenerationAutomatique
                     && !t.IsArchived)
            .ToListAsync(ct);

        if (!typesFraisAuto.Any()) return;

        var anneeScolaire = await _context.AnneeScolaires
            .Include(a => a.Periodes)
            .FirstOrDefaultAsync(a => a.Id == anneeScolaireId && a.IsActive, ct);

        if (anneeScolaire is null) return;

        var fraisListe = new List<Frais>();

        foreach (var typeFrais in typesFraisAuto)
        {
            if (!typeFrais.MontantsParNiveau.TryGetValue(niveau, out var montant))
                continue;

            if (typeFrais.Categorie == CategorieFrais.Inscription)
            {
                // Frais unique à l'inscription
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
                // Frais répartis sur les trimestres
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
                    // Pas de trimestres → frais annuel unique
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
        }
    }
}