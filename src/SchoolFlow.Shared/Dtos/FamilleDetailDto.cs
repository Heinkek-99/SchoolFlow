namespace SchoolFlow.Shared.Dtos;

public record FamilleDetailDto(
    Guid Id,
    string NomPere,
    string? PrenomPere,
    string? TelephonePere,
    string? EmailPere,
    string? NomMere,
    string? PrenomMere,
    string? TelephoneMere,
    string Adresse,
    string Ville,
    string TelephonePrincipal,
    List<EnfantDto> Enfants,
    decimal TotalDu,
    decimal TotalPaye,
    decimal SoldeGlobal
);