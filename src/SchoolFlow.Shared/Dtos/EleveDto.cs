namespace SchoolFlow.Shared.Dtos;

public record EleveDto(
    Guid Id,
    string Matricule,
    string Nom,
    string Prenom,
    string Classe,
    string Famille,
    decimal Solde,
    string Statut
);