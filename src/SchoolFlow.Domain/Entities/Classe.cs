namespace SchoolFlow.Domain.Entities;

public class Classe : BaseEntity
{
    public string Code { get; set; } = string.Empty; // "6EME_A"
    public string Nom { get; set; } = string.Empty; // "6ème A"
    public Niveau Niveau { get; set; }
    public string? Section { get; set; } // "A", "B", "Scientifique"
    public int CapaciteMax { get; set; } = 50;
    public int EffectifActuel => Eleves.Count(e => !e.IsArchived);
    
    public Guid AnneeScolaireId { get; set; }
    
    // Navigation
    public AnneeScolaire AnneeScolaire { get; set; } = null!;
    public ICollection<Eleve> Eleves { get; set; } = new List<Eleve>();
}

public enum Niveau
{
    CP = 1,
    CE1 = 2,
    CE2 = 3,
    CM1 = 4,
    CM2 = 5,
    SixiÚme = 6,
    CinquiÚme = 7,
    QuatriÚme = 8,
    TroisiÚme = 9,
    Seconde = 10,
    PremiÚre = 11,
    Terminale = 12
}