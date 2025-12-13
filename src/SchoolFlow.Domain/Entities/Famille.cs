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
    
    // Nom complet de la famille (basé sur le père ou la mère)
    public string NomFamille => 
    !string.IsNullOrWhiteSpace(NomPere) 
        ? $"{NomPere} {PrenomPere ?? ""}".Trim()
        : $"{NomMere ?? "Famille"}".Trim();

    // Calculs financiers
    public decimal TotalDu => Eleves.Sum(e => e.TotalDu);
    public decimal TotalPaye => Eleves.Sum(e => e.TotalPaye);
    public decimal SoldeGlobal => TotalDu - TotalPaye;
    
    // Pourcentage de recouvrement de la famille
    public decimal TauxRecouvrement => 
        TotalDu > 0 ? Math.Round((TotalPaye / TotalDu) * 100, 2) : 0;

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

    /// <summary>
    /// Nombre d'enfants actifs dans la famille
    /// </summary>
    public int NombreEnfantsActifs => 
        Eleves.Count(e => !e.IsArchived && e.Statut == StatutEleve.Actif);

    /// <summary>
    /// Date du dernier paiement effectué
    /// </summary>
    public DateTime? DateDernierPaiement => 
        Paiements.Any() ? Paiements.Max(p => p.DatePaiement) : null;

    /// <summary>
    /// Nombre de jours depuis le dernier paiement
    /// </summary>
    public int? JoursDepuisDernierPaiement => 
        DateDernierPaiement.HasValue 
            ? (DateTime.Now - DateDernierPaiement.Value).Days 
            : null;
}

public enum StatutPaiement
{
    Paye = 1,
    Partiel = 2,
    Impaye = 3
}