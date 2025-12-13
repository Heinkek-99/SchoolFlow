namespace SchoolFlow.Shared.Dtos;

public record FraisDto(
    Guid Id, 
    string Libelle, 
    decimal Montant, 
    decimal MontantPaye, 
    DateTime Echeance, 
    bool IsEchu,
    string? Periode
);
