namespace SchoolFlow.Shared.Dtos;

public record EleveDossierDto(
    Guid Id,
    string Matricule,
    string Nom,
    string Prenom,
    DateTime DateNaissance,
    string LieuNaissance,
    string Sexe,
    string? PhotoPath,
    string Classe,
    string Famille,
    List<FraisDto> Frais,
    decimal TotalDu,
    decimal TotalPaye,
    decimal Solde
);