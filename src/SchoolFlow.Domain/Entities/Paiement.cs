namespace SchoolFlow.Domain.Entities;

public class Paiement : BaseEntity
{
    public string NumeroPaiement { get; set; } = string.Empty; // PAY-2025-00456
    public Guid FamilleId { get; set; }
    public decimal MontantTotal { get; set; }
    public DateTime DatePaiement { get; set; } = DateTime.UtcNow;
    public ModePaiement ModePaiement { get; set; }
    public string? Reference { get; set; } // Numéro chèque, référence virement
    public string? Commentaire { get; set; }
    
    public Guid EnregistrePar { get; set; }
    
    // Navigation
    public Famille Famille { get; set; } = null!;
    public Utilisateur EnregistreParUtilisateur { get; set; } = null!;
    public ICollection<VentilationPaiement> Ventilations { get; set; } = new List<VentilationPaiement>();
}

public enum ModePaiement
{
    Especes = 1,
    Cheque = 2,
    Virement = 3,
    MobileMoney = 4,
    CarteCredit = 5
}