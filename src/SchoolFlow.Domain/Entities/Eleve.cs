namespace SchoolFlow.Domain.Entities;

public class Eleve : BaseEntity
{
    public string Matricule { get; set; } = string.Empty; // EL2025-00123
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public DateTime DateNaissance { get; set; }
    public string LieuNaissance { get; set; } = string.Empty;
    public Sexe Sexe { get; set; }
    public string? PhotoPath { get; set; }
    
    public Guid FamilleId { get; set; }
    public Guid ClasseId { get; set; }
    public Guid AnneeScolaireId { get; set; }
    
    // Infos complémentaires
    public string? Nationalite { get; set; } = "Camerounaise";
    public string? GroupeSanguin { get; set; }
    public string? Allergies { get; set; }
    public string? ContactUrgence { get; set; }
    
    // Statut
    public StatutEleve Statut { get; set; } = StatutEleve.Actif;
    public DateTime DateInscription { get; set; } = DateTime.UtcNow;
    
    // Calculs financiers
    public decimal TotalDu => Frais.Where(f => !f.IsArchived).Sum(f => f.Montant);
    public decimal TotalPaye => Frais.Where(f => !f.IsArchived).Sum(f => f.MontantPaye);
    public decimal Solde => TotalDu - TotalPaye;
    
    // Navigation
    public Famille Famille { get; set; } = null!;
    public Classe Classe { get; set; } = null!;
    public AnneeScolaire AnneeScolaire { get; set; } = null!;
    public ICollection<Frais> Frais { get; set; } = new List<Frais>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
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