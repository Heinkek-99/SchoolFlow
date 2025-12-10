namespace SchoolFlow.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid UtilisateurId { get; set; }
    public string Action { get; set; } = string.Empty; // "CREATE_ELEVE", "PAIEMENT_ENREGISTRE"
    public string EntityType { get; set; } = string.Empty; // "Eleve", "Paiement"
    public Guid? EntityId { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string IpAddress { get; set; } = string.Empty;
    
    public Utilisateur Utilisateur { get; set; } = null!;
}