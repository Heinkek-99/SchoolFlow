namespace SchoolFlow.Domain.Entities;

/// <summary>
/// Entité racine représentant un établissement scolaire.
/// C'est le "tenant" du système — toutes les données sont isolées par Ecole.
/// Inspiration : DigiSchool -> Ecole.java avec StatutEcole, TypeEtablissement
/// </summary>
public class Ecole : BaseEntity
{
    // === INFORMATIONS GÉNÉRALES ===
    public string Nom { get; private set; } = string.Empty;
    public string CodeEcole { get; private set; } = string.Empty; // Ex: SF-CMR-001
    public string Slug { get; private set; } = string.Empty;
    public TypeEtablissement Type { get; private set; }
    public TypeSecteur TypeSecteur { get; private set; } = TypeSecteur.PriveLaic;
    public SousSysteme SousSysteme { get; private set; } = SousSysteme.Francophone;
    public string CouleurPrimaire { get; private set; } = "#2563EB";
    public string CouleurSecondaire { get; private set; } = "#10B981";
    public string? Slogan { get; private set; }
    public string? LogoPath { get; private set; }

    // === COORDONNÉES ===
    public string Adresse { get; private set; } = string.Empty;
    public string Ville { get; private set; } = string.Empty;
    public string? Quartier { get; private set; }
    public string? Region { get; private set; }
    public string Pays { get; private set; } = "Cameroun";
    
    // === CONTACTS ===
    public string TelephonePrincipal { get; private set; } = string.Empty;
    public string? TelephoneSecondaire { get; private set; }
    public string? Email { get; private set; }
    public string? SiteWeb { get; private set; }

    // === STATUT ===
    public StatutEcole Statut { get; private set; } = StatutEcole.EnAttente;
    public DateTime? DateValidation { get; private set; }
    public string? MotifRejet { get; private set; }

    // === RESPONSABLE ===
    public string NomDirecteur { get; private set; } = string.Empty;
    public string? PrenomDirecteur { get; private set; }
    public string? TelephoneDirecteur { get; private set; }
    public string? EmailDirecteur { get; private set; }

    // === NAVIGATION ===
    public ICollection<Utilisateur> Utilisateurs { get; private set; } = new List<Utilisateur>();
    public ICollection<AnneeScolaire> AnneeScolaires { get; private set; } = new List<AnneeScolaire>();

    // === FACTORY METHOD (meilleure pratique DDD) ===
    
    /// <summary>
    /// Crée une nouvelle école et lève le domain event correspondant.
    /// </summary>
    public static Ecole Creer(
        string nom,
        TypeEtablissement type,
        string adresse,
        string ville,
        string? quartier,
        string? region,
        string pays,
        string telephonePrincipal,
        string? email,
        string nomDirecteur,
        string prenomDirecteur,
        string? telephoneDirecteur = null,
        string? emailDirecteur = null)
    {
        if (string.IsNullOrWhiteSpace(nom))
            throw new ArgumentException("Le nom de l'école est obligatoire.", nameof(nom));
 
        var ecole = new Ecole
        {
            Nom = nom.Trim(),
            Type = type,
            Adresse = adresse,
            Ville = ville,
            Quartier = quartier,
            Region = region,
            Pays = pays,
            TelephonePrincipal = telephonePrincipal,
            Email = email,
            NomDirecteur = nomDirecteur,
            PrenomDirecteur = prenomDirecteur,
            TelephoneDirecteur = telephoneDirecteur,
            EmailDirecteur = emailDirecteur,
            CodeEcole = GenererCode(nom, ville)
        };
 
        // 🔔 Domain Event — traçabilité + seed data déclenchée
        ecole.RaiseDomainEvent(new EcoleCreeeEvent(
            ecole.Id, ecole.Nom, ecole.CodeEcole, ecole.Type));
 
        return ecole;
    }
 
    public void Valider()
    {
        if (Statut == StatutEcole.Active)
            throw new InvalidOperationException("L'école est déjà active.");
 
        Statut = StatutEcole.Active;
        DateValidation = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
 
        RaiseDomainEvent(new EcoleValideeEvent(Id, Nom));
    }
 
    public void Rejeter(string motif)
    {
        if (string.IsNullOrWhiteSpace(motif))
            throw new ArgumentException("Le motif de rejet est obligatoire.");
 
        Statut = StatutEcole.Rejetee;
        MotifRejet = motif;
        UpdatedAt = DateTime.UtcNow;
    }
 
    public void MettreAJourLogo(string logoPath)
    {
        LogoPath = logoPath;
        UpdatedAt = DateTime.UtcNow;
    }
 
    public void Suspendre(string raison)
    {
        if (Statut != StatutEcole.Active)
            throw new InvalidOperationException("Seule une école active peut être suspendue.");

        Statut = StatutEcole.Suspendue;
        MotifRejet = raison;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MettreAJour(string? slogan, string? siteWeb,
        string? telephoneSecondaire, string? email)
    {
        Slogan = slogan;
        SiteWeb = siteWeb;
        TelephoneSecondaire = telephoneSecondaire;
        Email = email ?? Email;
        UpdatedAt = DateTime.UtcNow;
    }

    // ─── HELPERS PRIVÉS ──────────────────────────────────────────────────────
 
    private static string GenererCode(string nom, string ville)
    {
        var nomCode = new string(nom.Where(char.IsLetter).Take(3).ToArray()).ToUpper();
        var villeCode = new string(ville.Where(char.IsLetter).Take(2).ToArray()).ToUpper();
        var suffix = Random.Shared.Next(100, 999);
        return $"SF-{nomCode}{villeCode}-{suffix}";
    }
}

// ─── ENUMS ───────────────────────────────────────────────────────────────────
public enum TypeSecteur { Public = 1, PriveLaic = 2, PriveConfessionnel = 3, PriveCommunautaire = 4 }

public enum TypeEtablissement
{
    EcolePrimaire = 1,
    College = 2,
    Lycee = 3,
    LyceeEtCollege = 4,
    EcolePrimaireCycle1Et2 = 5
}

public enum StatutEcole
{
    EnAttente = 1,
    Active = 2,
    Suspendue = 3,
    Rejetee = 4
}