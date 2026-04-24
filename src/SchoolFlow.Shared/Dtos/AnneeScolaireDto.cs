namespace SchoolFlow.Shared.Dtos;

public record AnneeScolaireDto(
    Guid Id,
    string Libelle,
    DateTime DateDebut,
    DateTime DateFin,
    bool IsActive,
    int NombrePeriodes,
    int NombreClasses,
    int NombreEleves
);

public record PeriodeDto(
    Guid Id,
    string Libelle,
    string Type,
    int Numero,
    DateTime DateDebut,
    DateTime DateFin,
    Guid AnneeScolaireId
);
