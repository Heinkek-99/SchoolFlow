namespace SchoolFlow.Shared.Dtos;

public record ClasseDto(
    Guid Id, 
    string Code, 
    string Nom, 
    string Niveau, 
    int Effectif, 
    int CapaciteMax,
    bool EstComplete
);
