namespace SchoolFlow.Domain.Entities;

public class Examen : TenantEntity
{
    public Guid AnneeScolaireId { get; set; }

    public string Nom { get; set; } = string.Empty;
    public TypeExamen Type { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public string? Description { get; set; }

    // Navigation
    public AnneeScolaire AnneeScolaire { get; set; } = null!;
    public ICollection<InscriptionExamen> Inscrits { get; set; } = new List<InscriptionExamen>();
}

public class InscriptionExamen : TenantEntity
{
    public Guid ExamenId { get; set; }
    public Guid EleveId { get; set; }
    public string? NumeroCandidat { get; set; }
    public bool Admis { get; set; }
    public decimal? MoyenneExamen { get; set; }
    public string? Mention { get; set; }

    public Examen Examen { get; set; } = null!;
    public Eleve Eleve { get; set; } = null!;
}

public enum TypeExamen
{
    CEP = 1,
    FSLC = 2,
    BEPC = 3,
    GCEO = 4,
    BAC = 5,
    GCEA = 6
}
