namespace SchoolFlow.Domain.Entities;

/// <summary>
/// Évaluation planifiée sur une classe pour une matière et une période.
/// </summary>
public class EvaluationPlanifiee : TenantEntity
{
    public Guid ClasseId { get; set; }
    public Guid MatiereId { get; set; }
    public Guid PeriodeId { get; set; }
    public Guid AnneeScolaireId { get; set; }

    public TypeEvaluation Type { get; set; }
    public DateTime DateEvaluation { get; set; }
    public string? Description { get; set; }
    public decimal NoteSur { get; set; } = 20;
    public bool EstPubliee { get; set; } = false;

    // Navigation
    public Classe Classe { get; set; } = null!;
    public Matiere Matiere { get; set; } = null!;
    public Periode Periode { get; set; } = null!;
    public ICollection<Note> Notes { get; set; } = new List<Note>();

    public void Publier()
    {
        EstPubliee = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
