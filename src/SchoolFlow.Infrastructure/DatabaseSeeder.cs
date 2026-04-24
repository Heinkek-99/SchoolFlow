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
            _logger.LogInformation("Base déjà seedée — skip.");
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

        _logger.LogInformation("Seed terminé — Victoire + Champions créés avec données de démo.");
    }

    // ─── HELPERS ─────────────────────────────────────────────────────────────

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
