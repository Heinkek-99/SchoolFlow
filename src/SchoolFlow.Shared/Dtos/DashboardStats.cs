namespace SchoolFlow.Shared.Dtos;

// ========================
// DASHBOARD DTOs
// ========================

public record DashboardStats(
    int NombreElevesActifs,
    int NombreFamilles,
    StatistiquesFinancieres Finances,
    List<AlerteDto> Alertes,
    List<StatistiqueClasseDto> RepartitionClasses,
    GraphiqueEvolutionDto EvolutionEncaissements
    // public List<RepartitionNiveau> RepartitionParNiveau { get; init; } = new();

);

public record StatistiquesFinancieres(
    decimal TotalAEncaisser,
    decimal TotalEncaisse,
    decimal SoldeRestant,
    decimal TauxRecouvrement,
    int NombreFamillesImpayes,
    decimal MontantMoyenImpaye
);

public record AlerteDto(
    string Type,
    string Message,
    string Severite,
    DateTime Date,
    Guid? EntityId
);

public record StatistiqueClasseDto(
    string NomClasse,
    int NombreEleves,
    decimal TauxRecouvrement
);

public record GraphiqueEvolutionDto(
    List<string> Labels,
    List<decimal> Encaissements,
    List<decimal> Objectifs
);

public record FamilleImpayeDto(
    Guid FamilleId,
    string NomFamille,
    string Telephone,
    int NombreEnfants,
    decimal MontantDu,
    decimal MontantPaye,
    decimal SoldeRestant,
    DateTime DerniereRelance,
    int JoursImpaye,
    string Priorite
);

public record StatsFinancieresDetailDto(
    decimal EncaissementsMois,
    decimal EncaissementsTrimestre,
    decimal EncaissementsAnnee,
    decimal ObjectifMois,
    decimal ObjectifAnnee,
    decimal TauxRealisationMois,
    decimal TauxRealisationAnnee,
    List<FamilleImpayeDto> FamillesImpayes,
    List<TopImpayeDto> Top10Impayes,
    List<EncaissementParModeDto> RepartitionModesPaiement
);

public record TopImpayeDto(
    int Rang,
    string NomFamille,
    decimal MontantImpaye,
    int NombreEnfants
);

public record EncaissementParModeDto(
    string ModePaiement,
    decimal Montant,
    int NombreTransactions,
    decimal Pourcentage
);