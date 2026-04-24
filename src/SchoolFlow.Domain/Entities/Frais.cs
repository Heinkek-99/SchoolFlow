namespace SchoolFlow.Domain.Entities;

/// <summary>
/// Représente un frais dû par un élève.
/// CORRECTION : ImputerMontant() vit ici, dans le Domain — pas dans le Handler.
/// </summary>
public class Frais : TenantEntity
{
    public Guid EleveId { get; set; }
    public Guid TypeFraisId { get; set; }
    public Guid? PeriodeId { get; set; }  // nullable — inscription n'a pas de période

    public decimal Montant { get;  set; }
    public decimal MontantPaye { get; set; }
    public DateTime DateEcheance { get; set; }
    public string? Description { get; set; }
    public string ? Remarques { get; set; }
    public string ? Commentaire { get; set; }

    // Calculés
    public decimal Solde => Montant - MontantPaye;
    public bool EstSolde => Solde <= 0.01m;
    public decimal PourcentagePaye => Montant > 0
        ? Math.Round(MontantPaye / Montant * 100, 2) : 0;
    public bool IsEchu => !EstSolde && DateEcheance < DateTime.UtcNow;
    public int JoursRetard => IsEchu ? (int)(DateTime.UtcNow - DateEcheance).TotalDays : 0;

    public StatutPaiementFrais StatutPaiement => EstSolde ? StatutPaiementFrais.Paye
        : MontantPaye > 0 ? StatutPaiementFrais.Partiel
        : IsEchu ? StatutPaiementFrais.Impaye
        : StatutPaiementFrais.EnAttente;
        

    // Navigation
    public Eleve Eleve { get; set; } = null!;
    public TypeFrais TypeFrais { get; set; } = null!;
    public Periode? Periode { get; set; }
    public ICollection<VentilationPaiement> Ventilations { get; set; } = new List<VentilationPaiement>();
    
    public static Frais Creer(
        Guid ecoleId,
        Guid eleveId,
        Guid typeFraisId,
        decimal montant,
        DateTime dateEcheance,
        string? description = null)
    {
        if (montant <= 0)
            throw new ArgumentException("Le montant doit être positif.", nameof(montant));

        return new Frais
        {
            EcoleId = ecoleId,
            EleveId = eleveId,
            TypeFraisId = typeFraisId,
            Montant = montant,
            DateEcheance = dateEcheance,
            Description = description
        };
    }

    /// <summary>
    /// Impute un montant sur ce frais.
    /// Logique métier ici dans le Domain — plus dans les handlers.
    /// </summary>
    public void ImputerMontant(decimal montant)
    {
        if (montant <= 0)
            throw new ArgumentException("Le montant imputé doit être positif.", nameof(montant));
        if (montant > Solde + 0.01m)
            throw new InvalidOperationException(
                $"Montant {montant:N0} FCFA dépasse le solde restant {Solde:N0} FCFA.");

        MontantPaye += montant;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum StatutPaiementFrais 
    { 
        EnAttente = 1,
        Partiel = 2, 
        Paye = 3,
        Impaye = 4
    }