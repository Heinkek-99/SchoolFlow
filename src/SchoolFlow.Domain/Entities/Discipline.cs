namespace SchoolFlow.Domain.Entities;

public class Discipline : TenantEntity
{
    public Guid EleveId { get; set; }
    public Guid AnneeScolaireId { get; set; }
    public Guid? SignalePar { get; set; }

    public TypeDiscipline Type { get; set; }
    public string Motif { get; set; } = string.Empty;
    public DateTime DateDiscipline { get; set; }
    public string? Mesure { get; set; }
    public bool NotifieParent { get; set; } = false;

    // Navigation
    public Eleve Eleve { get; set; } = null!;
    public AnneeScolaire AnneeScolaire { get; set; } = null!;
    public Utilisateur? SignaleParUtilisateur { get; set; }
}

public enum TypeDiscipline
{
    Avertissement = 1,
    Blame = 2,
    RenvoyiTemporaire = 3,
    RenvoyiDefinitif = 4,
    Retenue = 5,
    Convocation = 6
}
