namespace SchoolFlow.Domain.Entities;

public class Famille : BaseEntity
{
    public string NomPere { get; set; } = string.Empty;
    public string? PrenomPere { get; set; }
    public string? TelephonePere { get; set; }
    public string? EmailPere { get; set; }
    public string? ProfessionPere { get; set; }
    
    public string? NomMere { get; set; }
    public string? PrenomMere { get; set; }
    public string? TelephoneMere { get; set; }
    public string? EmailMere { get; set; }
    public string? ProfessionMere { get; set; }
    
    public string Adresse { get; set; } = string.Empty;
    public string Ville { get; set; } = string.Empty;
    public string? QuartierCommune { get; set; }
    
    public string TelephonePrincipal { get; set; } = string.Empty;
    public string? TelephoneSecondaire { get; set; }
    
    // Calculs financiers
    public decimal TotalDu => Eleves.Sum(e => e.TotalDu);
    public decimal TotalPaye => Eleves.Sum(e => e.TotalPaye);
    public decimal SoldeGlobal => TotalDu - TotalPaye;
    public StatutPaiement StatutPaiement => CalculerStatut();
    
    // Navigation
    public ICollection<Eleve> Eleves { get; set; } = new List<Eleve>();
    public ICollection<Paiement> Paiements { get; set; } = new List<Paiement>();
    
    private StatutPaiement CalculerStatut()
    {
        if (SoldeGlobal <= 0) return StatutPaiement.Paye;
        if (TotalPaye > 0) return StatutPaiement.Partiel;
        return StatutPaiement.Impaye;
    }
}

public enum StatutPaiement
{
    Paye = 1,
    Partiel = 2,
    Impaye = 3
}