using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolFlow.Domain.Entities;
using SchoolFlow.Infrastructure.Data;

namespace SchoolFlow.Infrastructure;

/// <summary>
/// Initialise les données de démonstration en DEV.
/// Appeler depuis Program.cs : await seeder.SeedAsync();
/// </summary>
public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        // SuperAdmin est seedé indépendamment des données école
        if (!await _context.Utilisateurs.AnyAsync(u => u.Role == Role.SuperAdmin, ct))
        {
            var superAdmin = Utilisateur.CreerSuperAdmin(
                username: "superadmin",
                passwordHash: BCrypt.Net.BCrypt.HashPassword("SuperAdmin@2025", workFactor: 12),
                nom: "SchoolFlow",
                prenom: "Admin",
                email: "admin@schoolflow.cm");
            _context.Utilisateurs.Add(superAdmin);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("SuperAdmin créé — superadmin / SuperAdmin@2025");
        }

        if (await _context.Ecoles.AnyAsync(ct))
        {
            // La DB existe déjà — vérifier seulement les données manquantes critiques
            await CorrigerDonneesManquantes(ct);
            _logger.LogInformation("Base déjà seedée — vérification des données manquantes effectuée.");
            return;
        }

        _logger.LogInformation("Initialisation des données de démonstration...");

        // ── ÉCOLE 1 : La Victoire Bilingue — Yaoundé ────────────────────────
        var ecoleVictoire = new Ecole();
        SetEcoleProps(ecoleVictoire,
            nom: "La Victoire Bilingue",
            codeEcole: "SF-VICTOIRE-001",
            type: TypeEtablissement.LyceeEtCollege,
            typeSecteur: TypeSecteur.PriveConfessionnel,
            sousSysteme: SousSysteme.Bilingue,
            adresse: "Bastos, Rue 1042",
            ville: "Yaoundé",
            pays: "Cameroun",
            telephone: "+237 690 000 001",
            nomDirecteur: "Nkengne",
            prenomDirecteur: "Paul");
        _context.Ecoles.Add(ecoleVictoire);

        // ── ÉCOLE 2 : Les Champions — Bafoussam ─────────────────────────────
        var ecoleChampions = new Ecole();
        SetEcoleProps(ecoleChampions,
            nom: "Les Champions",
            codeEcole: "SF-CHAMPIONS-002",
            type: TypeEtablissement.College,
            typeSecteur: TypeSecteur.PriveLaic,
            sousSysteme: SousSysteme.Francophone,
            adresse: "Centre-ville, Av. des Martyrs",
            ville: "Bafoussam",
            pays: "Cameroun",
            telephone: "+237 690 000 002",
            nomDirecteur: "Kamga",
            prenomDirecteur: "Marie");
        _context.Ecoles.Add(ecoleChampions);

        await _context.SaveChangesAsync(ct);

        // ── UTILISATEURS — École Victoire ────────────────────────────────────
        var admin = Utilisateur.Creer(
            ecoleId: ecoleVictoire.Id,
            username: "admin.victoire",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("Admin@2025", workFactor: 12),
            nom: "Nkengne", prenom: "Paul", role: Role.Admin,
            email: "admin@victoire.cm");

        var secretaire = Utilisateur.Creer(
            ecoleId: ecoleVictoire.Id,
            username: "secretaire.victoire",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("Secr@2025", workFactor: 12),
            nom: "Fotso", prenom: "Jeanne", role: Role.Secretaire,
            email: "secretaire@victoire.cm");

        var comptable = Utilisateur.Creer(
            ecoleId: ecoleVictoire.Id,
            username: "comptable.victoire",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("Compt@2025", workFactor: 12),
            nom: "Nguele", prenom: "Eric", role: Role.Comptable,
            email: "comptable@victoire.cm");

        _context.Utilisateurs.AddRange(admin, secretaire, comptable);

        // ── ANNÉE SCOLAIRE ───────────────────────────────────────────────────
        var annee = new AnneeScolaire
        {
            EcoleId = ecoleVictoire.Id,
            Libelle = "2024-2025",
            DateDebut = new DateTime(2024, 9, 2, 0, 0, 0, DateTimeKind.Utc),
            DateFin = new DateTime(2025, 7, 31, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true
        };
        _context.AnneeScolaires.Add(annee);
        await _context.SaveChangesAsync(ct);

        // Trimestres
        var t1 = new Periode { EcoleId = ecoleVictoire.Id, AnneeScolaireId = annee.Id, Libelle = "1er Trimestre", Type = TypePeriode.Trimestre, Numero = 1, DateDebut = new DateTime(2024, 9, 2, 0, 0, 0, DateTimeKind.Utc), DateFin = new DateTime(2024, 12, 20, 0, 0, 0, DateTimeKind.Utc) };
        var t2 = new Periode { EcoleId = ecoleVictoire.Id, AnneeScolaireId = annee.Id, Libelle = "2ème Trimestre", Type = TypePeriode.Trimestre, Numero = 2, DateDebut = new DateTime(2025, 1, 6, 0, 0, 0, DateTimeKind.Utc), DateFin = new DateTime(2025, 3, 28, 0, 0, 0, DateTimeKind.Utc) };
        var t3 = new Periode { EcoleId = ecoleVictoire.Id, AnneeScolaireId = annee.Id, Libelle = "3ème Trimestre", Type = TypePeriode.Trimestre, Numero = 3, DateDebut = new DateTime(2025, 4, 7, 0, 0, 0, DateTimeKind.Utc), DateFin = new DateTime(2025, 7, 4, 0, 0, 0, DateTimeKind.Utc) };
        _context.Periodes.AddRange(t1, t2, t3);
        await _context.SaveChangesAsync(ct);

        // ── CLASSES ──────────────────────────────────────────────────────────
        var cm2a = new Classe { EcoleId = ecoleVictoire.Id, AnneeScolaireId = annee.Id, Code = "CM2-A", Nom = "CM2", Section = "A", Niveau = Niveau.CM2, SousSysteme = SousSysteme.Francophone, CapaciteMax = 45, Statut = StatutClasse.Active };
        var class6a = new Classe { EcoleId = ecoleVictoire.Id, AnneeScolaireId = annee.Id, Code = "Class6-A", Nom = "Class 6", Section = "A", Niveau = Niveau.Form1, SousSysteme = SousSysteme.Anglophone, CapaciteMax = 40, Statut = StatutClasse.Active };
        var sixa = new Classe { EcoleId = ecoleVictoire.Id, AnneeScolaireId = annee.Id, Code = "6ème-A", Nom = "6ème", Section = "A", Niveau = Niveau.Sixieme, SousSysteme = SousSysteme.Francophone, CapaciteMax = 45, Statut = StatutClasse.Active };
        var sixb = new Classe { EcoleId = ecoleVictoire.Id, AnneeScolaireId = annee.Id, Code = "6ème-B", Nom = "6ème", Section = "B", Niveau = Niveau.Sixieme, SousSysteme = SousSysteme.Francophone, CapaciteMax = 45, Statut = StatutClasse.Active };
        var tled = new Classe { EcoleId = ecoleVictoire.Id, AnneeScolaireId = annee.Id, Code = "Tle-D", Nom = "Terminale", Section = "D", Niveau = Niveau.Terminale, SousSysteme = SousSysteme.Francophone, CapaciteMax = 35, Statut = StatutClasse.Active };
        _context.Classes.AddRange(cm2a, class6a, sixa, sixb, tled);

        // ── TYPES DE FRAIS ───────────────────────────────────────────────────
        var montantsScol = new Dictionary<Niveau, decimal>
        {
            { Niveau.CM2, 95_000 }, { Niveau.Sixieme, 120_000 },
            { Niveau.Form1, 140_000 }, { Niveau.Terminale, 180_000 }
        };
        var tfInscription = new TypeFrais
        {
            EcoleId = ecoleVictoire.Id, Code = "INSCR", Libelle = "Frais d'inscription",
            Categorie = CategorieFrais.Inscription, IsObligatoire = true, GenerationAutomatique = true,
            MontantsParNiveau = montantsScol.ToDictionary(k => k.Key, v => Math.Round(v.Value * 0.2m, 0))
        };
        var tfScolarite = new TypeFrais
        {
            EcoleId = ecoleVictoire.Id, Code = "SCOL", Libelle = "Frais de scolarité",
            Categorie = CategorieFrais.Scolarite, IsObligatoire = true, GenerationAutomatique = true,
            MontantsParNiveau = montantsScol
        };
        _context.TypeFrais.AddRange(tfInscription, tfScolarite);
        await _context.SaveChangesAsync(ct);

        // ── MATIÈRES PAR DÉFAUT — École Victoire (Bilingue) ─────────────────
        await SeedMatieresAsync(ecoleVictoire.Id, SousSysteme.Bilingue, ct);

        // ── FAMILLE NKOULOU ──────────────────────────────────────────────────
        var familleNkoulou = Famille.Creer(
            ecoleId: ecoleVictoire.Id,
            nomPere: "Nkoulou", prenomPere: "Georges",
            telephonePere: "+237 699 001 001", emailPere: null, professionPere: "Ingénieur",
            nomMere: "Nkoulou", prenomMere: "Cécile",
            telephoneMere: "+237 699 001 002",
            adresse: "Bastos, Rue des Manguiers", ville: "Yaoundé",
            quartierCommune: "Bastos", telephonePrincipal: "+237 699 001 001");
        _context.Familles.Add(familleNkoulou);

        // ── FAMILLE MANGA ────────────────────────────────────────────────────
        var familleManga = Famille.Creer(
            ecoleId: ecoleVictoire.Id,
            nomPere: "Manga", prenomPere: "Henri",
            telephonePere: "+237 699 002 001", emailPere: null, professionPere: "Commerçant",
            nomMere: null, prenomMere: null, telephoneMere: null,
            adresse: "Mvog-Ada", ville: "Yaoundé",
            quartierCommune: "Mvog-Ada", telephonePrincipal: "+237 699 002 001");
        _context.Familles.Add(familleManga);
        await _context.SaveChangesAsync(ct);

        // ── ÉLÈVES Nkoulou ───────────────────────────────────────────────────
        var paul = new Eleve
        {
            EcoleId = ecoleVictoire.Id,
            Matricule = "EL2025-00001",
            Nom = "Nkoulou", Prenom = "Paul",
            DateNaissance = new DateTime(2012, 3, 15, 0, 0, 0, DateTimeKind.Utc),
            LieuNaissance = "Yaoundé", Sexe = Sexe.Masculin,
            FamilleId = familleNkoulou.Id, ClasseId = sixa.Id, AnneeScolaireId = annee.Id,
            Statut = StatutEleve.Actif, DateInscription = DateTime.UtcNow
        };
        var marie = new Eleve
        {
            EcoleId = ecoleVictoire.Id,
            Matricule = "EL2025-00002",
            Nom = "Nkoulou", Prenom = "Marie",
            DateNaissance = new DateTime(2010, 7, 22, 0, 0, 0, DateTimeKind.Utc),
            LieuNaissance = "Yaoundé", Sexe = Sexe.Feminin,
            FamilleId = familleNkoulou.Id, ClasseId = tled.Id, AnneeScolaireId = annee.Id,
            Statut = StatutEleve.Actif, DateInscription = DateTime.UtcNow
        };

        // ── ÉLÈVES Manga ─────────────────────────────────────────────────────
        var junior = new Eleve
        {
            EcoleId = ecoleVictoire.Id,
            Matricule = "EL2025-00003",
            Nom = "Manga", Prenom = "Junior",
            DateNaissance = new DateTime(2013, 1, 10, 0, 0, 0, DateTimeKind.Utc),
            LieuNaissance = "Douala", Sexe = Sexe.Masculin,
            FamilleId = familleManga.Id, ClasseId = cm2a.Id, AnneeScolaireId = annee.Id,
            Statut = StatutEleve.Actif, DateInscription = DateTime.UtcNow
        };
        var grace = new Eleve
        {
            EcoleId = ecoleVictoire.Id,
            Matricule = "EL2025-00004",
            Nom = "Manga", Prenom = "Grâce",
            DateNaissance = new DateTime(2011, 9, 5, 0, 0, 0, DateTimeKind.Utc),
            LieuNaissance = "Douala", Sexe = Sexe.Feminin,
            FamilleId = familleManga.Id, ClasseId = sixa.Id, AnneeScolaireId = annee.Id,
            Statut = StatutEleve.Actif, DateInscription = DateTime.UtcNow
        };

        _context.Eleves.AddRange(paul, marie, junior, grace);
        await _context.SaveChangesAsync(ct);

        // ── FRAIS ────────────────────────────────────────────────────────────
        var frais = new List<Frais>();

        foreach (var (eleve, niveau) in new[] {
            (paul, Niveau.Sixieme), (marie, Niveau.Terminale),
            (junior, Niveau.CM2), (grace, Niveau.Sixieme)
        })
        {
            // Inscription
            if (tfInscription.MontantsParNiveau.TryGetValue(niveau, out var mInscr))
                frais.Add(new Frais { EcoleId = ecoleVictoire.Id, EleveId = eleve.Id, TypeFraisId = tfInscription.Id, Montant = mInscr, DateEcheance = new DateTime(2024, 9, 15, 0, 0, 0, DateTimeKind.Utc) });

            // Scolarité — 3 trimestres
            if (tfScolarite.MontantsParNiveau.TryGetValue(niveau, out var mScol))
            {
                var parTrimestre = Math.Round(mScol / 3, 0);
                frais.Add(new Frais { EcoleId = ecoleVictoire.Id, EleveId = eleve.Id, TypeFraisId = tfScolarite.Id, PeriodeId = t1.Id, Montant = parTrimestre, DateEcheance = new DateTime(2024, 9, 30, 0, 0, 0, DateTimeKind.Utc) });
                frais.Add(new Frais { EcoleId = ecoleVictoire.Id, EleveId = eleve.Id, TypeFraisId = tfScolarite.Id, PeriodeId = t2.Id, Montant = parTrimestre, DateEcheance = new DateTime(2025, 1, 20, 0, 0, 0, DateTimeKind.Utc) });
                frais.Add(new Frais { EcoleId = ecoleVictoire.Id, EleveId = eleve.Id, TypeFraisId = tfScolarite.Id, PeriodeId = t3.Id, Montant = parTrimestre, DateEcheance = new DateTime(2025, 4, 20, 0, 0, 0, DateTimeKind.Utc) });
            }
        }

        _context.Frais.AddRange(frais);
        await _context.SaveChangesAsync(ct);

        // ── PAIEMENT NKOULOU : 70 000 FCFA (Paul 35k + Marie 35k) ───────────
        var fraisPaul = frais.Where(f => f.EleveId == paul.Id).OrderBy(f => f.DateEcheance).ToList();
        var fraisMarie = frais.Where(f => f.EleveId == marie.Id).OrderBy(f => f.DateEcheance).ToList();

        var paiement = Paiement.Creer(
            ecoleId: ecoleVictoire.Id,
            numeroPaiement: "PAY-2024-00001",
            familleId: familleNkoulou.Id,
            montantTotal: 70_000,
            datePaiement: new DateTime(2024, 9, 20, 0, 0, 0, DateTimeKind.Utc),
            modePaiement: ModePaiement.MobileMoney,
            enregistrePar: comptable.Id,
            commentaire: "Paiement partiel à l'inscription");

        _context.Paiements.Add(paiement);
        await _context.SaveChangesAsync(ct);

        // Ventilations manuelles (35k chacun)
        var resteP = 35_000m;
        foreach (var f in fraisPaul)
        {
            if (resteP <= 0.01m) break;
            var imputer = Math.Min(resteP, f.Solde);
            if (imputer <= 0) continue;
            f.ImputerMontant(imputer);
            _context.VentilationsPaiement.Add(new VentilationPaiement { EcoleId = ecoleVictoire.Id, PaiementId = paiement.Id, EleveId = paul.Id, FraisId = f.Id, MontantVentile = imputer });
            resteP -= imputer;
        }

        var resteM = 35_000m;
        foreach (var f in fraisMarie)
        {
            if (resteM <= 0.01m) break;
            var imputer = Math.Min(resteM, f.Solde);
            if (imputer <= 0) continue;
            f.ImputerMontant(imputer);
            _context.VentilationsPaiement.Add(new VentilationPaiement { EcoleId = ecoleVictoire.Id, PaiementId = paiement.Id, EleveId = marie.Id, FraisId = f.Id, MontantVentile = imputer });
            resteM -= imputer;
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("""

            ╔══════════════════════════════════════════════════════════════╗
            ║              SCHOOLFLOW — COMPTES DE DÉMO                   ║
            ╠══════════════╦══════════════════╦═════════════╦═════════════╣
            ║ Username     ║ Mot de passe     ║ Rôle        ║ École       ║
            ╠══════════════╬══════════════════╬═════════════╬═════════════╣
            ║ superadmin   ║ SuperAdmin@2025  ║ SuperAdmin  ║ —           ║
            ║ admin.vic... ║ Admin@2025       ║ Admin       ║ La Victoire ║
            ║ secret.vic.. ║ Secr@2025        ║ Secrétaire  ║ La Victoire ║
            ║ compt.vic... ║ Compt@2025       ║ Comptable   ║ La Victoire ║
            ╚══════════════╩══════════════════╩═════════════╩═════════════╝
            """);
    }

    // ─── CORRECTION ECOLE_ID CORROMPUS ───────────────────────────────────────────

    public async Task CorrigerEcoleIdAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Vérification intégrité EcoleId...");

        var ecoleVictoire = await _context.Ecoles
            .FirstOrDefaultAsync(e => e.CodeEcole == "SF-VICTOIRE-001", ct);

        if (ecoleVictoire is null)
        {
            _logger.LogWarning("École La Victoire introuvable — skip correction EcoleId.");
            return;
        }

        var guid0 = Guid.Empty;

        // Corriger Familles avec EcoleId = Guid.Empty
        var familles = await _context.Familles
            .IgnoreQueryFilters()
            .Where(f => f.EcoleId == guid0)
            .ToListAsync(ct);

        _logger.LogInformation("Familles à corriger : {Count}", familles.Count);

        foreach (var f in familles)
        {
            var eleveSource = await _context.Eleves
                .Where(e => e.FamilleId == f.Id && e.EcoleId != guid0)
                .Select(e => e.EcoleId)
                .FirstOrDefaultAsync(ct);

            f.EcoleId = eleveSource != Guid.Empty ? eleveSource : ecoleVictoire.Id;
        }

        if (familles.Count > 0)
        {
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("{Count} famille(s) EcoleId corrigé(s)", familles.Count);
        }

        // Corriger TypeFrais avec EcoleId = Guid.Empty
        var typesFrais = await _context.TypeFrais
            .IgnoreQueryFilters()
            .Where(t => t.EcoleId == guid0)
            .ToListAsync(ct);

        foreach (var tf in typesFrais)
            tf.EcoleId = ecoleVictoire.Id;

        if (typesFrais.Count > 0)
        {
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("{Count} TypeFrais EcoleId corrigé(s)", typesFrais.Count);
        }

        // Corriger Utilisateurs avec EcoleId = Guid.Empty (hors SuperAdmin)
        var utilisateurs = await _context.Utilisateurs
            .IgnoreQueryFilters()
            .Where(u => u.EcoleId == guid0 && u.Role != Role.SuperAdmin)
            .ToListAsync(ct);

        foreach (var u in utilisateurs)
            u.EcoleId = ecoleVictoire.Id;

        if (utilisateurs.Count > 0)
        {
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("{Count} utilisateur(s) EcoleId corrigé(s)", utilisateurs.Count);
        }

        // Corriger AnneeScolaires avec EcoleId = Guid.Empty
        var annees = await _context.AnneeScolaires
            .IgnoreQueryFilters()
            .Where(a => a.EcoleId == guid0)
            .ToListAsync(ct);

        foreach (var a in annees)
            a.EcoleId = ecoleVictoire.Id;

        if (annees.Count > 0)
        {
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("{Count} AnneeScolaire(s) EcoleId corrigé(s)", annees.Count);
        }

        // Corriger Classes avec EcoleId = Guid.Empty
        var classes = await _context.Classes
            .IgnoreQueryFilters()
            .Where(c => c.EcoleId == guid0)
            .ToListAsync(ct);

        foreach (var c in classes)
            c.EcoleId = ecoleVictoire.Id;

        if (classes.Count > 0)
        {
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("{Count} classe(s) EcoleId corrigé(s)", classes.Count);
        }

        _logger.LogInformation("Correction EcoleId terminée.");
    }

    // ─── CORRECTION DONNÉES MANQUANTES ───────────────────────────────────────────

    private async Task CorrigerDonneesManquantes(CancellationToken ct)
    {
        var ecoleVictoire = await _context.Ecoles
            .FirstOrDefaultAsync(e => e.CodeEcole == "SF-VICTOIRE-001", ct);

        if (ecoleVictoire is null) return;

        // TypesFrais manquants pour La Victoire
        if (!await _context.TypeFrais.AnyAsync(tf => tf.EcoleId == ecoleVictoire.Id, ct))
        {
            _logger.LogInformation("Création des TypesFrais manquants pour La Victoire...");

            var montants = new Dictionary<Niveau, decimal>
            {
                { Niveau.CM2, 95_000 }, { Niveau.Sixieme, 120_000 },
                { Niveau.Form1, 140_000 }, { Niveau.Terminale, 180_000 }
            };

            _context.TypeFrais.AddRange(
                new TypeFrais
                {
                    EcoleId = ecoleVictoire.Id, Code = "INSCR", Libelle = "Frais d'inscription",
                    Categorie = CategorieFrais.Inscription, IsObligatoire = true, GenerationAutomatique = true,
                    MontantsParNiveau = montants.ToDictionary(k => k.Key, v => Math.Round(v.Value * 0.2m, 0))
                },
                new TypeFrais
                {
                    EcoleId = ecoleVictoire.Id, Code = "SCOL", Libelle = "Frais de scolarité",
                    Categorie = CategorieFrais.Scolarite, IsObligatoire = true, IsRecurrent = true,
                    GenerationAutomatique = true, MontantsParNiveau = montants
                }
            );
            await _context.SaveChangesAsync(ct);
        }

        // Matières manquantes — pour TOUTES les écoles actives
        var toutesEcoles = await _context.Ecoles
            .Where(e => e.Statut == StatutEcole.Active)
            .ToListAsync(ct);

        foreach (var ecole in toutesEcoles)
            await SeedMatieresAsync(ecole.Id, ecole.SousSysteme, ct);

        // AnneeScolaire manquante ou inactive
        if (!await _context.AnneeScolaires.AnyAsync(a => a.EcoleId == ecoleVictoire.Id && a.IsActive, ct))
        {
            var existante = await _context.AnneeScolaires
                .FirstOrDefaultAsync(a => a.EcoleId == ecoleVictoire.Id, ct);

            if (existante is null)
            {
                _logger.LogInformation("Création AnneeScolaire 2024-2025 manquante pour La Victoire...");
                _context.AnneeScolaires.Add(new AnneeScolaire
                {
                    EcoleId = ecoleVictoire.Id, Libelle = "2024-2025",
                    DateDebut = new DateTime(2024, 9, 2, 0, 0, 0, DateTimeKind.Utc),
                    DateFin = new DateTime(2025, 7, 31, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                });
                await _context.SaveChangesAsync(ct);
            }
            else
            {
                _logger.LogWarning(
                    "AnneeScolaire '{Libelle}' existe pour La Victoire mais IsActive=false — activation...",
                    existante.Libelle);
                existante.IsActive = true;
                await _context.SaveChangesAsync(ct);
            }
        }
    }

    private async Task SeedMatieresAsync(Guid ecoleId, SousSysteme sousSysteme, CancellationToken ct)
    {
        if (await _context.Matieres.AnyAsync(m => m.EcoleId == ecoleId, ct)) return;

        (string Code, string Libelle, decimal Coeff)[] matieres = sousSysteme switch
        {
            SousSysteme.Francophone =>
            [
                ("MATH", "Mathématiques", 4m), ("FR",   "Français", 4m),
                ("PC",   "Physique-Chimie", 3m), ("SVT", "Sciences de la Vie et de la Terre", 3m),
                ("HG",   "Histoire-Géographie", 2m), ("ANG", "Anglais", 2m),
                ("EPS",  "Éducation Physique", 1m), ("ICT", "Informatique", 1m),
                ("EC",   "Économie", 2m), ("PHILO", "Philosophie", 2m)
            ],
            SousSysteme.Anglophone =>
            [
                ("MATH", "Mathematics", 4m), ("ENG", "English", 4m),
                ("SCI",  "Science", 3m), ("HIST", "History", 2m),
                ("GEO",  "Geography", 2m), ("FR",  "French", 2m),
                ("ICT",  "ICT", 1m), ("PE",  "Physical Education", 1m)
            ],
            _ =>  // Bilingue
            [
                ("MATH", "Mathématiques / Mathematics", 4m), ("FR", "Français", 3m),
                ("ENG",  "English", 3m), ("PC", "Physique-Chimie / Science", 3m),
                ("SVT",  "Sciences de la Vie", 2m), ("HG", "Histoire-Géographie", 2m),
                ("ICT",  "Informatique", 1m), ("EPS", "Éducation Physique", 1m)
            ]
        };

        _context.Matieres.AddRange(matieres.Select(m => new Matiere
        {
            EcoleId = ecoleId,
            Code = m.Code,
            Libelle = m.Libelle,
            Coefficient = m.Coeff,
            SousSysteme = sousSysteme,
            IsActive = true
        }));

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Matières seedées ({Count}) pour EcoleId={EcoleId}", matieres.Length, ecoleId);
    }

    public async Task NettoyerDonneesTestAsync()
    {
        // Matières de test (code TZ*)
        var matTest = await _context.Matieres
            .Where(m => m.Code.StartsWith("TZ"))
            .ToListAsync();
        if (matTest.Any())
        {
            _context.Matieres.RemoveRange(matTest);
            await _context.SaveChangesAsync();
        }

        // Années scolaires de test (libellé contient "-test-")
        var anneesTest = await _context.AnneeScolaires
            .Where(a => a.Libelle.Contains("-test-"))
            .ToListAsync();
        if (anneesTest.Any())
        {
            _context.AnneeScolaires.RemoveRange(anneesTest);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Nettoyage: {Count} année(s) de test supprimée(s).", anneesTest.Count);
        }

        // Classes de test
        var classesTest = await _context.Classes
            .Where(c => c.Code.Contains("-TEST"))
            .ToListAsync();
        if (classesTest.Any())
        {
            _context.Classes.RemoveRange(classesTest);
            await _context.SaveChangesAsync();
        }

        // Disciplines avec date invalide (avant 2000) → corriger avec CreatedAt
        var cutoff = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var discInvalides = await _context.Disciplines
            .Where(d => d.DateDiscipline < cutoff)
            .ToListAsync();
        foreach (var d in discInvalides)
            d.DateDiscipline = d.CreatedAt;
        if (discInvalides.Any())
            await _context.SaveChangesAsync();

        // Dédoublonner évaluations (garder la plus récente par classe+matière+période+année+type)
        await _context.Database.ExecuteSqlRawAsync("""
            DELETE FROM "Evaluations"
            WHERE "Id" NOT IN (
                SELECT "Id" FROM (
                    SELECT DISTINCT ON ("ClasseId","MatiereId","PeriodeId","AnneeScolaireId","Type") "Id"
                    FROM "Evaluations"
                    ORDER BY "ClasseId","MatiereId","PeriodeId","AnneeScolaireId","Type","CreatedAt"
                ) kept
            )
            """);

        // Dédoublonner disciplines (garder la plus récente par élève+motif+type)
        await _context.Database.ExecuteSqlRawAsync("""
            DELETE FROM "Disciplines"
            WHERE "Id" NOT IN (
                SELECT "Id" FROM (
                    SELECT DISTINCT ON ("EleveId","Motif","Type") "Id"
                    FROM "Disciplines"
                    ORDER BY "EleveId","Motif","Type","CreatedAt"
                ) kept
            )
            """);

        // Dédoublonner inscriptions examen puis examens (cascade gère InscriptionsExamen)
        await _context.Database.ExecuteSqlRawAsync("""
            DELETE FROM "InscriptionsExamen"
            WHERE "ExamenId" NOT IN (
                SELECT "Id" FROM (
                    SELECT DISTINCT ON ("Nom","AnneeScolaireId") "Id"
                    FROM "Examens"
                    ORDER BY "Nom","AnneeScolaireId","CreatedAt"
                ) kept
            )
            """);
        await _context.Database.ExecuteSqlRawAsync("""
            DELETE FROM "Examens"
            WHERE "Id" NOT IN (
                SELECT "Id" FROM (
                    SELECT DISTINCT ON ("Nom","AnneeScolaireId") "Id"
                    FROM "Examens"
                    ORDER BY "Nom","AnneeScolaireId","CreatedAt"
                ) kept
            )
            """);

        // Dédoublonner créneaux horaires
        await _context.Database.ExecuteSqlRawAsync("""
            DELETE FROM "CreneauxHoraires"
            WHERE "Id" NOT IN (
                SELECT "Id" FROM (
                    SELECT DISTINCT ON ("ClasseId","MatiereId","Jour","HeureDebut") "Id"
                    FROM "CreneauxHoraires"
                    ORDER BY "ClasseId","MatiereId","Jour","HeureDebut","CreatedAt"
                ) kept
            )
            """);

        _logger.LogInformation("Données de test nettoyées.");
    }

    // ─── HELPERS ─────────────────────────────────────────────────────────────────

    private static void SetEcoleProps(Ecole e,
        string nom, string codeEcole, TypeEtablissement type,
        TypeSecteur typeSecteur, SousSysteme sousSysteme,
        string adresse, string ville, string pays,
        string telephone, string nomDirecteur, string prenomDirecteur)
    {
        // Accès direct via reflection pour bypass les private setters du seed
        typeof(Ecole).GetProperty(nameof(Ecole.Nom))!.SetValue(e, nom);
        typeof(Ecole).GetProperty(nameof(Ecole.CodeEcole))!.SetValue(e, codeEcole);
        typeof(Ecole).GetProperty(nameof(Ecole.Slug))!.SetValue(e, nom.ToLower().Replace(" ", "-"));
        typeof(Ecole).GetProperty(nameof(Ecole.Type))!.SetValue(e, type);
        typeof(Ecole).GetProperty(nameof(Ecole.TypeSecteur))!.SetValue(e, typeSecteur);
        typeof(Ecole).GetProperty(nameof(Ecole.SousSysteme))!.SetValue(e, sousSysteme);
        typeof(Ecole).GetProperty(nameof(Ecole.Adresse))!.SetValue(e, adresse);
        typeof(Ecole).GetProperty(nameof(Ecole.Ville))!.SetValue(e, ville);
        typeof(Ecole).GetProperty(nameof(Ecole.Pays))!.SetValue(e, pays);
        typeof(Ecole).GetProperty(nameof(Ecole.TelephonePrincipal))!.SetValue(e, telephone);
        typeof(Ecole).GetProperty(nameof(Ecole.NomDirecteur))!.SetValue(e, nomDirecteur);
        typeof(Ecole).GetProperty(nameof(Ecole.PrenomDirecteur))!.SetValue(e, prenomDirecteur);
        typeof(Ecole).GetProperty(nameof(Ecole.Statut))!.SetValue(e, StatutEcole.Active);
        typeof(Ecole).GetProperty(nameof(Ecole.DateValidation))!.SetValue(e, DateTime.UtcNow);
    }
}
