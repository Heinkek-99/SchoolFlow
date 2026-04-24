namespace SchoolFlow.Shared.Dtos;

public record TypeFraisDto(
    Guid Id,
    string Code,
    string Libelle,
    string? Description,
    string Categorie,
    bool IsRecurrent,
    bool IsObligatoire,
    bool GenerationAutomatique,
    Dictionary<string, decimal> MontantsParNiveau
);
