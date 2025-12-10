namespace SchoolFlow.Shared.Dtos;

public record FamilleDto(
    Guid Id,
    string NomPere,
    string? PrenomPere,
    string TelephonePrincipal,
    string Ville,
    int NombreEnfants,
    decimal TotalDu,
    decimal TotalPaye,
    decimal SoldeGlobal,
    string StatutPaiement
);