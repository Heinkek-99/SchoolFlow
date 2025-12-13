namespace SchoolFlow.Shared.Dtos;

public record EleveSimpleDto(
    Guid Id,
    string Matricule, 
    string NomComplet,
    string Sexe,
    decimal Solde

);