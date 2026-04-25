namespace SchoolFlow.Domain.Entities;

public class Matiere : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public decimal Coefficient { get; set; } = 1;
    public SousSysteme SousSysteme { get; set; } = SousSysteme.Francophone;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Note> Notes { get; set; } = new List<Note>();
    public ICollection<CreneauHoraire> Creneaux { get; set; } = new List<CreneauHoraire>();
    public ICollection<EvaluationPlanifiee> Evaluations { get; set; } = new List<EvaluationPlanifiee>();
}
