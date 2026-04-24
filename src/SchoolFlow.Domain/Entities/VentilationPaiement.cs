namespace SchoolFlow.Domain.Entities;

public class VentilationPaiement : TenantEntity  // ← était BaseEntity
{
    public Guid PaiementId { get; set; }
    public Guid EleveId { get; set; }
    public Guid FraisId { get; set; }
    public decimal MontantVentile { get; set; }
    public string? Remarque { get; set; }
 
    // Navigation
    public Paiement Paiement { get; set; } = null!;
    public Eleve Eleve { get; set; } = null!;
    public Frais Frais { get; set; } = null!;
}