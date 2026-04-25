// ============================================================
// FICHIER : src/SchoolFlow.Application/Ecoles/Handlers/CreerEcoleCommandHandler.cs
// ACTION  : Fichier à CRÉER
// ============================================================

using BCrypt.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Ecoles.Commands;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Ecoles.Handlers;

/// <summary>
/// Crée une école + Admin initial + Seed data (classes + types de frais).
/// 
/// Flow complet :
/// 1. Vérifications unicité
/// 2. Ecole.Creer() → lève EcoleCreeeEvent
/// 3. Utilisateur Admin créé (BCrypt)
/// 4. Seed : classes selon type d'établissement
/// 5. Seed : types de frais standards
/// 6. SaveChanges → OutboxMessage créé automatiquement → Event publié
/// </summary>
public class CreerEcoleCommandHandler
    : IRequestHandler<CreerEcoleCommand, Result<CreerEcoleResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreerEcoleCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<CreerEcoleResponse>> Handle(
        CreerEcoleCommand request, CancellationToken ct)
    {
        // ── 1. VÉRIFICATIONS ────────────────────────────────────────────────
        var nomNormalise = request.NomEcole.Trim();

        var nomLower = nomNormalise.ToLower();
        var villeLower = request.Ville.Trim().ToLower();
        var ecoleExistante = await _context.Ecoles
            .AnyAsync(e => e.Nom.ToLower() == nomLower
                        && e.Ville.ToLower() == villeLower, ct);

        if (ecoleExistante)
            return Result<CreerEcoleResponse>.Failure(
                $"Une école '{nomNormalise}' existe déjà à {request.Ville}.");

        var usernameExistant = await _context.Utilisateurs
            .AnyAsync(u => u.Username == request.UsernameAdmin.ToLower(), ct);

        if (usernameExistant)
            return Result<CreerEcoleResponse>.Failure(
                $"Le nom d'utilisateur '{request.UsernameAdmin}' est déjà utilisé.");

        // Validation du mot de passe Admin
        if (request.PasswordAdmin.Length < 8)
            return Result<CreerEcoleResponse>.Failure(
                "Le mot de passe doit contenir au moins 8 caractères.");

        // ── 2. CRÉER L'ÉCOLE ────────────────────────────────────────────────
        var ecole = Ecole.Creer(
            nom: nomNormalise,
            type: request.TypeEtablissement,
            adresse: request.Adresse,
            ville: request.Ville,
            quartier: request.Quartier,
            region: request.Region,
            pays: request.Pays,
            telephonePrincipal: request.TelephonePrincipal,
            email: request.Email,
            nomDirecteur: request.NomDirecteur,
            prenomDirecteur: request.PrenomDirecteur,
            telephoneDirecteur: request.TelephoneDirecteur,
            emailDirecteur: request.EmailDirecteur
        );

        _context.Ecoles.Add(ecole);

        // ── 3. CRÉER L'ADMIN INITIAL ────────────────────────────────────────
        var admin = Utilisateur.Creer(
            ecoleId: ecole.Id,
            username: request.UsernameAdmin,
            passwordHash: BCrypt.Net.BCrypt.HashPassword(request.PasswordAdmin, workFactor: 12),
            nom: request.NomDirecteur,
            prenom: request.PrenomDirecteur,
            role: Role.Admin,
            email: request.EmailDirecteur,
            telephone: request.TelephoneDirecteur
        );

        _context.Utilisateurs.Add(admin);

        // ── 4. SEED : CLASSES par défaut ────────────────────────────────────
        var annee = new AnneeScolaire
        {
            EcoleId = ecole.Id,
            Libelle = $"{DateTime.UtcNow.Year}-{DateTime.UtcNow.Year + 1}",
            DateDebut = new DateTime(DateTime.UtcNow.Year, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            DateFin = new DateTime(DateTime.UtcNow.Year + 1, 7, 31, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true
        };
        _context.AnneeScolaires.Add(annee);

        var classes = GenererClasses(ecole.Id, annee.Id, request.TypeEtablissement);
        _context.Classes.AddRange(classes);

        // ── 5. SEED : TYPES DE FRAIS standards ──────────────────────────────
        var typesFrais = GenererTypesFrais(ecole.Id);
        _context.TypeFrais.AddRange(typesFrais);

        // ── 6. SAUVEGARDER (OutboxMessage créé automatiquement par DbContext) ──
        await _context.SaveChangesAsync(ct);

        return Result<CreerEcoleResponse>.Success(new CreerEcoleResponse(
            EcoleId: ecole.Id,
            CodeEcole: ecole.CodeEcole,
            NomEcole: ecole.Nom,
            Statut: ecole.Statut,
            Message: $"École '{ecole.Nom}' créée. Code : {ecole.CodeEcole}. " +
                     "En attente de validation. Votre compte Admin est prêt."
        ));
    }

    // ─── SEED : Classes ──────────────────────────────────────────────────────

    private static List<Classe> GenererClasses(
        Guid ecoleId, Guid anneeId, TypeEtablissement type)
    {
        var niveaux = type switch
        {
            TypeEtablissement.EcolePrimaire => new[]
            {
                (Niveau.CP, "CP", "Cours Préparatoire"),
                (Niveau.CE1, "CE1", "Cours Élémentaire 1"),
                (Niveau.CE2, "CE2", "Cours Élémentaire 2"),
                (Niveau.CM1, "CM1", "Cours Moyen 1"),
                (Niveau.CM2, "CM2", "Cours Moyen 2"),
            },
            TypeEtablissement.College => new[]
            {
                (Niveau.Sixieme, "6ème", "Sixième"),
                (Niveau.Cinquieme, "5ème", "Cinquième"),
                (Niveau.Quatrieme, "4ème", "Quatrième"),
                (Niveau.Troisieme, "3ème", "Troisième"),
            },
            TypeEtablissement.Lycee => new[]
            {
                (Niveau.Seconde, "2nde", "Seconde"),
                (Niveau.Premiere, "1ère", "Première"),
                (Niveau.Terminale, "Tle", "Terminale"),
            },
            _ => new[]
            {
                (Niveau.Sixieme, "6ème", "Sixième"),
                (Niveau.Cinquieme, "5ème", "Cinquième"),
                (Niveau.Quatrieme, "4ème", "Quatrième"),
                (Niveau.Troisieme, "3ème", "Troisième"),
                (Niveau.Seconde, "2nde", "Seconde"),
                (Niveau.Premiere, "1ère", "Première"),
                (Niveau.Terminale, "Tle", "Terminale"),
            }
        };

        return niveaux.Select((n, i) => new Classe
        {
            EcoleId = ecoleId,
            AnneeScolaireId = anneeId,
            Niveau = n.Item1,
            Code = n.Item2,
            Nom = n.Item3,
            CapaciteMax = 50
        }).ToList();
    }

    // ─── SEED : Types de frais ───────────────────────────────────────────────

    private static List<TypeFrais> GenererTypesFrais(Guid ecoleId)
    {
        // Montants par défaut en FCFA (ajustables par l'école)
        var montantsNiveau = new Dictionary<Niveau, decimal>
        {
            { Niveau.CP, 90_000 },    { Niveau.CE1, 90_000 },
            { Niveau.CE2, 90_000 },   { Niveau.CM1, 95_000 },
            { Niveau.CM2, 95_000 },   { Niveau.Sixieme, 120_000 },
            { Niveau.Cinquieme, 120_000 }, { Niveau.Quatrieme, 130_000 },
            { Niveau.Troisieme, 130_000 }, { Niveau.Seconde, 150_000 },
            { Niveau.Premiere, 160_000 },  { Niveau.Terminale, 180_000 }
        };

        var montantsInscription = montantsNiveau.ToDictionary(
            k => k.Key, v => Math.Round(v.Value * 0.2m, 0)); // 20% des frais de scolarité

        return new List<TypeFrais>
        {
            new()
            {
                EcoleId = ecoleId,
                Code = "INSCR",
                Libelle = "Frais d'inscription",
                Description = "Frais d'inscription annuel (non remboursable)",
                Categorie = CategorieFrais.Inscription,
                IsRecurrent = false,
                IsObligatoire = true,
                GenerationAutomatique = true,
                MontantsParNiveau = montantsInscription
            },
            new()
            {
                EcoleId = ecoleId,
                Code = "SCOL",
                Libelle = "Frais de scolarité",
                Description = "Frais de scolarité annuel (en 3 tranches trimestrielles)",
                Categorie = CategorieFrais.Scolarite,
                IsRecurrent = true,
                IsObligatoire = true,
                GenerationAutomatique = true,
                MontantsParNiveau = montantsNiveau
            },
            new()
            {
                EcoleId = ecoleId,
                Code = "CANT",
                Libelle = "Frais de cantine",
                Description = "Repas de midi — optionnel",
                Categorie = CategorieFrais.Cantine,
                IsRecurrent = true,
                IsObligatoire = false,
                GenerationAutomatique = false,
                MontantsParNiveau = montantsNiveau.ToDictionary(k => k.Key, _ => 50_000m)
            },
            new()
            {
                EcoleId = ecoleId,
                Code = "TRANS",
                Libelle = "Frais de transport",
                Description = "Navette scolaire — optionnel",
                Categorie = CategorieFrais.Transport,
                IsRecurrent = true,
                IsObligatoire = false,
                GenerationAutomatique = false,
                MontantsParNiveau = montantsNiveau.ToDictionary(k => k.Key, _ => 40_000m)
            },
            new()
            {
                EcoleId = ecoleId,
                Code = "EXAM",
                Libelle = "Frais d'examen",
                Description = "Examens officiels (BEPC, BAC, etc.)",
                Categorie = CategorieFrais.Annexe,
                IsRecurrent = false,
                IsObligatoire = false,
                GenerationAutomatique = false,
                MontantsParNiveau = new Dictionary<Niveau, decimal>
                {
                    { Niveau.Troisieme, 25_000 },
                    { Niveau.Terminale, 35_000 }
                }
            }
        };
    }
}