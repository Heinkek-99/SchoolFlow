namespace SchoolFlow.Domain.Entities;

public class Eleve : TenantEntity
{
    public string Matricule { get; set; } = string.Empty; // EL2025-00123
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public DateTime DateNaissance { get; set; }
    public string LieuNaissance { get; set; } = string.Empty;
    public Sexe Sexe { get; set; }
    public string? PhotoPath { get; set; }
    
    public Guid FamilleId { get; set; }
    public Guid? ClasseId { get; set; }
    public Guid? AnneeScolaireId { get; set; }
    
    // Infos complémentaires
    public string? Nationalite { get; set; } = "Camerounaise";
    public string? GroupeSanguin { get; set; }
    public string? Allergies { get; set; }
    public string? ContactUrgence { get; set; }
    
    // Remarques
    public string? Remarques { get; set; }

    // Statut
    public StatutEleve Statut { get; set; } = StatutEleve.Actif;
    public DateTime DateInscription { get; set; } = DateTime.UtcNow;
    
    // Calculs financiers

    // Nom complet de l'élève
    public string NomComplet => $"{Prenom} {Nom}";

    // Âge de l'élève en années
    public int Age
    {
        get
        {
            var today = DateTime.Today;
            var age = today.Year - DateNaissance.Year;
            if (DateNaissance.Date > today.AddYears(-age)) age--;
            return age;
        }
    }

    // Total des frais dus par l'élève
    public decimal TotalDu => Frais.Where(f => !f.IsArchived).Sum(f => f.Montant);

    // Total des montants déjà payés
    public decimal TotalPaye => Frais.Where(f => !f.IsArchived).Sum(f => f.MontantPaye);

    // Solde restant à payer
    public decimal Solde => TotalDu - TotalPaye;

    // Indique si l'élève est à jour dans ses paiements
    public bool EstAJour => Solde <= 0.01m;

    // Pourcentage de paiement
    public decimal PourcentagePaye => 
        TotalDu > 0 ? Math.Round((TotalPaye / TotalDu) * 100, 2) : 0;

    // Nombre de frais impayés
    public int NombreFraisImpayes => 
        Frais.Count(f => !f.IsArchived && f.Solde > 0.01m);

    // Frais le plus ancien non payé
    public Frais? FraisPlusAncienImpaye => 
        Frais
            .Where(f => !f.IsArchived && f.Solde > 0.01m)
            .OrderBy(f => f.DateEcheance)
            .FirstOrDefault();

    // // Moyenne générale de l'élève (si notes disponibles)
    // public decimal? MoyenneGenerale
    // {
    //     get
    //     {
    //         var notesValides = Notes
    //             .Where(n => n.Valeur.HasValue && n.NoteSur > 0)
    //             .ToList();

    //         if (!notesValides.Any()) return null;

    //         var totalPoints = notesValides.Sum(n => (n.Valeur!.Value / n.NoteSur) * 20);
    //         return Math.Round(totalPoints / notesValides.Count, 2);
    //     }
    // }


    // Navigation
    public Famille Famille { get; set; } = null!;
    public Classe? Classe { get; set; } = null!;
    public AnneeScolaire? AnneeScolaire { get; set; } = null!;
    public ICollection<Frais> Frais { get; set; } = new List<Frais>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();

    public static Eleve Inscrire(
        string nom, string prenom, DateTime dateNaissance, string lieuNaissance,
        Sexe sexe, Guid familleId, Guid classeId, Guid anneeScolaireId,
        Guid ecoleId, string matricule,
        string? photoPath = null, string? nationalite = null,
        string? groupeSanguin = null, string? allergies = null,
        string? contactUrgence = null)
    {
        var eleve = new Eleve
        {
            EcoleId = ecoleId,
            Matricule = matricule,
            Nom = nom,
            Prenom = prenom,
            DateNaissance = dateNaissance,
            LieuNaissance = lieuNaissance,
            Sexe = sexe,
            FamilleId = familleId,
            ClasseId = classeId,
            AnneeScolaireId = anneeScolaireId,
            PhotoPath = photoPath,
            Nationalite = nationalite ?? "Camerounaise",
            GroupeSanguin = groupeSanguin,
            Allergies = allergies,
            ContactUrgence = contactUrgence,
            Statut = StatutEleve.Actif,
            DateInscription = DateTime.UtcNow
        };

        eleve.RaiseDomainEvent(new EleveInscritEvent(
            eleve.Id, matricule, familleId, ecoleId, classeId, anneeScolaireId));

        return eleve;
    }
}

public enum Sexe
{
    Masculin = 1,
    Feminin = 2
}

public enum StatutEleve
{
    Actif = 1,
    Suspendu = 2,
    Radie = 3,
    Diplome = 4
}