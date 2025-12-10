namespace SchoolFlow.Domain.Entities;

public class VentilationPaiement : BaseEntity
{
    public Guid PaiementId { get; set; }
    public Guid FraisId { get; set; }
    public decimal MontantVentile { get; set; }
    
    // Navigation
    public Paiement Paiement { get; set; } = null!;
    public Frais Frais { get; set; } = null!;
}