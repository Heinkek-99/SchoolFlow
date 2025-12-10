namespace SchoolFlow.Domain.Entities;

public class TypeFrais : BaseEntity
{
    public string Code { get; set; } = string.Empty; // "SCOL_T1"
    public string Libelle { get; set; } = string.Empty; // "Scolarité Trimestre 1"
    public string? Description { get; set; }
    public CategorieFrais Categorie { get; set; }
    public bool IsRecurrent { get; set; } = false;
    public bool IsObligatoire { get; set; } = true;
    public bool GenerationAutomatique { get; set; } = true;
    
    // Montants par niveau (JSON serialized)
    public Dictionary<Niveau, decimal> MontantsParNiveau { get; set; } = new();
    
    // Navigation
    public ICollection<Frais> Frais { get; set; } = new List<Frais>();
}

public enum CategorieFrais
{
    Inscription = 1,
    Scolarite = 2,
    Cantine = 3,
    Uniforme = 4,
    Transport = 5,
    Annexe = 99
}
