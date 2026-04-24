namespace SchoolFlow.Domain.Entities;

public class AnneeScolaire : TenantEntity  // ← était BaseEntity
{
    public string Libelle { get; set; } = string.Empty;  // "2024-2025"
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public bool IsActive { get; set; } = false;

    // Navigation
    public Ecole Ecole { get; set; } = null!;
    public ICollection<Classe> Classes { get; set; } = new List<Classe>();
    public ICollection<Eleve> Eleves { get; set; } = new List<Eleve>();
    public ICollection<Periode> Periodes { get; set; } = new List<Periode>();

    // Méthode métier
    public void Activer()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Desactiver()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class Periode : TenantEntity  // ← était BaseEntity
{
    public string Libelle { get; set; } = string.Empty;  // "Trimestre 1"
    public TypePeriode Type { get; set; }
    public int Numero { get; set; }  // 1, 2, 3
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }

    public Guid AnneeScolaireId { get; set; }
    public AnneeScolaire AnneeScolaire { get; set; } = null!;
}

public enum TypePeriode
{
    Trimestre = 1,
    Semestre = 2
}