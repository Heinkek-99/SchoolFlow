namespace SchoolFlow.Domain.Entities;

public class Frais : BaseEntity
{
    public Guid EleveId { get; set; }
    public Guid TypeFraisId { get; set; }
    public Guid? PeriodeId { get; set; }
    
    public decimal Montant { get; set; }
    public decimal MontantPaye { get; set; } // Somme des montants ventilés via les paiements
    public DateTime DateEcheance { get; set; }
    public string? Remarques { get; set; }

    public decimal Solde => Montant - MontantPaye;
    public bool EstSolde => Solde <= 0.01m;
    
    public bool IsEchu => DateTime.UtcNow > DateEcheance && Solde > 0;
    
    public string? Commentaire { get; set; }

    public int JoursRetard => IsEchu ? (DateTime.Now - DateEcheance).Days : 0;
    
    // Pourcentage payé
    public decimal PourcentagePaye => Montant > 0 ? Math.Round((MontantPaye / Montant) * 100, 2) : 0;
    
    // Statut du paiement
    public StatutPaiementFrais StatutPaiement
    {
        get
        {
            if (EstSolde) return StatutPaiementFrais.Paye;
            if (IsEchu) return StatutPaiementFrais.Impaye;
            if (MontantPaye > 0) return StatutPaiementFrais.Partiel;
            return StatutPaiementFrais.EnAttente;
        }
    }
    
    // Navigation
    public Eleve Eleve { get; set; } = null!;
    public TypeFrais TypeFrais { get; set; } = null!;
    public Periode? Periode { get; set; }
    public ICollection<VentilationPaiement> Ventilations { get; set; } = new List<VentilationPaiement>();
}


/// <summary>
/// Statuts possibles d'un frais
/// </summary>
public enum StatutPaiementFrais
{
    EnAttente,  // Pas encore payé, pas échu
    Partiel,    // Partiellement payé
    Paye,       // Totalement payé
    Impaye      // Échu et non payé
}