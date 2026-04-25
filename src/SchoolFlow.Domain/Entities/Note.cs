namespace SchoolFlow.Domain.Entities;

public class Note : TenantEntity
{
    public Guid EleveId { get; set; }
    public Guid MatiereId { get; set; }
    public Guid PeriodeId { get; set; }
    public Guid? EvaluationId { get; set; }

    public decimal Valeur { get; set; }
    public decimal NoteSur { get; set; } = 20;
    public TypeEvaluation Type { get; set; }
    public string? Commentaire { get; set; }
    public bool EstPubliee { get; set; } = false;

    public decimal ValeurSur20 => NoteSur > 0 ? Math.Round((Valeur / NoteSur) * 20, 2) : 0;
    public string Appreciation => ValeurSur20 switch
    {
        >= 16 => "Très Bien",
        >= 14 => "Bien",
        >= 12 => "Assez Bien",
        >= 10 => "Passable",
        >= 8  => "Insuffisant",
        _     => "Médiocre"
    };

    // Navigation
    public Eleve Eleve { get; set; } = null!;
    public Matiere Matiere { get; set; } = null!;
    public Periode Periode { get; set; } = null!;
    public EvaluationPlanifiee? Evaluation { get; set; }

    public void Publier()
    {
        EstPubliee = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum TypeEvaluation
{
    Interrogation = 1,
    Devoir = 2,
    Composition = 3,
    Examen = 4
}
