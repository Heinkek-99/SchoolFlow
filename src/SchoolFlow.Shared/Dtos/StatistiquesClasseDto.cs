namespace SchoolFlow.Shared.Dtos;

public record StatistiquesClasseDto(
    int EffectifTotal,
    int NombreGarcons,
    int NombreFilles,
    decimal TauxRemplissage,
    decimal MoyenneAge,
    decimal TotalDu,
    decimal TotalPaye,
    decimal TauxRecouvrement,
    int ElevesAJour,
    int ElevesImpayés
);