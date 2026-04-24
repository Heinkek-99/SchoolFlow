namespace SchoolFlow.Shared.Dtos;

public record ClasseDto(
    Guid Id,
    string Code,
    string Nom,
    string NomComplet,
    string Niveau,
    string SousSysteme,
    string? Section,
    int Effectif,
    int CapaciteMax,
    int PlacesDisponibles,
    bool EstComplete,
    decimal? FraisScolarite,
    string Statut,
    Guid AnneeScolaireId,
    string LibelleAnnee
);
