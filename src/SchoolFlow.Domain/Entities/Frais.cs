namespace SchoolFlow.Domain.Entities;

public class Frais : BaseEntity
{
    public Guid EleveId { get; set; }
    public Guid TypeFraisId { get; set; }
    public Guid? PeriodeId { get; set; }
    
    public decimal Montant { get; set; }
    public decimal MontantPaye { get; set; } = 0;
    public decimal Solde => Montant - MontantPaye;
    
    public DateTime DateEcheance { get; set; }
    public bool IsEchu => DateTime.UtcNow > DateEcheance && Solde > 0;
    
    public string? Commentaire { get; set; }
    
    // Navigation
    public Eleve Eleve { get; set; } = null!;
    public TypeFrais TypeFrais { get; set; } = null!;
    public Periode? Periode { get; set; }
    public ICollection<VentilationPaiement> Ventilations { get; set; } = new List<VentilationPaiement>();
}