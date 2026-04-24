namespace SchoolFlow.Domain.Entities;

public class Utilisateur : BaseEntity  // hérite de BaseEntity — EcoleId nullable pour SuperAdmin
{
    public Guid? EcoleId { get; set; }  // null uniquement pour SuperAdmin

    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telephone { get; set; }
    public Role Role { get; set; } = Role.Secretaire;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockedUntil { get; set; }

    public string NomComplet => $"{Prenom} {Nom}";
    public bool EstVerrouille => LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;

    // Navigation
    public Ecole? Ecole { get; set; }  // null pour SuperAdmin
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();


// ─── FACTORY METHODS ──────────────────────────────────────────────────────

    public static Utilisateur Creer(
        Guid ecoleId,
        string username,
        string passwordHash,
        string nom,
        string prenom,
        Role role,
        string? email = null,
        string? telephone = null)
    {
        return new Utilisateur
        {
            EcoleId = ecoleId,
            Username = username.Trim().ToLower(),
            PasswordHash = passwordHash,
            Nom = nom.Trim(),
            Prenom = prenom.Trim(),
            Role = role,
            Email = email?.Trim().ToLower(),
            Telephone = telephone,
            IsActive = true
        };
    }

    public static Utilisateur CreerSuperAdmin(
        string username,
        string passwordHash,
        string nom,
        string prenom,
        string? email = null)
    {
        return new Utilisateur
        {
            EcoleId = null,
            Username = username.Trim().ToLower(),
            PasswordHash = passwordHash,
            Nom = nom.Trim(),
            Prenom = prenom.Trim(),
            Role = Role.SuperAdmin,
            Email = email?.Trim().ToLower(),
            IsActive = true
        };
    }
 
    // ─── MÉTHODES MÉTIER ─────────────────────────────────────────────────────
 
    public void EnregistrerConnexion()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }
 
    public bool EnregistrerEchecConnexion(int maxTentatives = 5, int dureeVerrouillageMn = 30)
    {
        FailedLoginAttempts++;
        UpdatedAt = DateTime.UtcNow;
 
        if (FailedLoginAttempts >= maxTentatives)
        {
            LockedUntil = DateTime.UtcNow.AddMinutes(dureeVerrouillageMn);
            return true; // compte verrouillé
        }
        return false;
    }
 
    public void Deverrouiller()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }
 
    public void Activer() { IsActive = true; UpdatedAt = DateTime.UtcNow; }
    public void Desactiver() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
 
    public void MettreAJourMotDePasse(string nouveauHash)
    {
        PasswordHash = nouveauHash;
        UpdatedAt = DateTime.UtcNow;
    }
 
    public void MettreAJour(string nom, string prenom, string? email, string? telephone)
    {
        Nom = nom.Trim();
        Prenom = prenom.Trim();
        Email = email?.Trim().ToLower();
        Telephone = telephone;
        UpdatedAt = DateTime.UtcNow;
    }
}
 
public enum Role
{
    SuperAdmin = 0,
    Admin = 1,
    Directeur = 2,
    Secretaire = 3,
    Comptable = 4
}