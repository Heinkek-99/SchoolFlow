
namespace SchoolFlow.Domain.Entities;

public class Utilisateur : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public Role Role { get; set; } = Role.Secretaire;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockedUntil { get; set; }
    
    // Navigation
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

}

public enum Role
{
    Admin = 1,
    Directeur = 2,
    Secretaire = 3,
    Comptable = 4
}