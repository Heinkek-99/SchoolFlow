namespace SchoolFlow.Domain.Entities;

public class Enseignant : TenantEntity
{
    public Guid UtilisateurId { get; set; }
    public string Telephone { get; set; } = string.Empty;
    public string? Specialite { get; set; }
    public string? Grade { get; set; }
    public int? AnneesExperience { get; set; }
    public string? Bio { get; set; }
    public string? PhotoPath { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Utilisateur Utilisateur { get; set; } = null!;
    public ICollection<MatiereEnseignant> Matieres { get; set; } = new List<MatiereEnseignant>();
    public ICollection<CreneauHoraire> Creneaux { get; set; } = new List<CreneauHoraire>();
}

/// <summary>Table de jointure Enseignant ↔ Matière (N:N) par classe et année</summary>
public class MatiereEnseignant : TenantEntity
{
    public Guid EnseignantId { get; set; }
    public Guid MatiereId { get; set; }
    public Guid ClasseId { get; set; }
    public Guid AnneeScolaireId { get; set; }

    public Enseignant Enseignant { get; set; } = null!;
    public Matiere Matiere { get; set; } = null!;
    public Classe Classe { get; set; } = null!;
}
