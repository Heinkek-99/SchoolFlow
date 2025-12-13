namespace SchoolFlow.Shared.Dtos;

public record EleveClasseDto(
    Guid Id,
    string Matricule,
    string NomComplet,
    string Sexe,
    int Age,
    decimal Solde,
    string StatutFinancier
);