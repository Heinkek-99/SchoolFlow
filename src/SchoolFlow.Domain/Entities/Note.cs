namespace SchoolFlow.Domain.Entities;

public class Note : BaseEntity
{
    public Guid EleveId { get; set; }
    public Guid MatiereId { get; set; }
    public Guid PeriodeId { get; set; }
    
    public decimal Valeur { get; set; }
    public decimal NoteSur { get; set; } = 20;
    public TypeEvaluation Type { get; set; }
    public string? Commentaire { get; set; }
    
    // Navigation
    public Eleve Eleve { get; set; } = null!;
    public Matiere Matiere { get; set; } = null!;
    public Periode Periode { get; set; } = null!;
}

public enum TypeEvaluation
{
    Interrogation = 1,
    Devoir = 2,
    Composition = 3
}

public class Matiere : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public int Coefficient { get; set; } = 1;
    public ICollection<Note> Notes { get; set; } = new List<Note>();
}
