namespace SchoolFlow.Shared.Dtos;

public record ClasseDetailDto(
    Guid Id,
    string Code,
    string Nom,
    string Niveau,
    string? Section,
    int Effectif,
    int CapaciteMax,
    List<EleveClasseDto> Eleves,
    string AnneeScolaire
);