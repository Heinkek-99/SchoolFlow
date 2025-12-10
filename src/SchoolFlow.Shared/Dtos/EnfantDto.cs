namespace SchoolFlow.Shared.Dtos;

public record EnfantDto(Guid Id, string Nom, string Prenom, string Matricule, string Classe, decimal Solde);
