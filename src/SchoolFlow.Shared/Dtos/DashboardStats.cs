namespace SchoolFlow.Shared.Dtos;

// ========================
// DASHBOARD DTOs
// ========================

public record DashboardStats(
    int TotalEleves,
    int TotalFamilles,
    StatistiquesFinancieres Finances,
    List<AlerteDto> Alertes,
    List<StatistiqueClasseDto> RepartitionClasses,
    GraphiqueEvolutionDto EvolutionEncaissements
    // public List<RepartitionNiveau> RepartitionParNiveau { get; init; } = new();

);


public record DashboardStatsDto(
    int TotalEleves,
    int TotalFamilles,
    decimal TotalFraisAttendus,
    decimal TotalEncaisse,
    decimal SoldeGlobal,
    decimal TauxRecouvrement,
    List<StatistiqueClasseDto> StatistiquesParClasse
);

public record StatistiquesFinancieres(
    decimal TotalAEncaisser,
    decimal TotalEncaisse,
    decimal SoldeRestant,
    decimal TauxRecouvrement,
    int TotalFamillesImpayes,
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
    DateTime? ProchaineEcheance,
    int JoursRetard
)
{
    /// <summary>
    /// Niveau de priorité (pour tri/affichage)
    /// </summary>
    public string NiveauPriorite => JoursRetard switch
    {
        > 30 => "Critique",
        > 15 => "Urgent",
        > 0 => "Attention",
        _ => "Normal"
    };

    /// <summary>
    /// Statut d'impayé
    /// </summary>
    public string StatutImpaie => JoursRetard switch
    {
        > 90 => "Très en retard",
        > 30 => "En retard",
        > 0 => "Échéance dépassée",
        _ => "Impayé à venir"
    };
};

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
    List<EncaissementParModeDto> Repartition
);

public record StatsFinancieresDto(
    decimal TotalAttendu,
    decimal TotalEncaisse,
    decimal SoldeGlobal,
    decimal PaiementsRecents30Jours,
    decimal TauxRecouvrement,
    List<RepartitionTypeFraisDto> RepartitionTypesFrais
)
{
    /// <summary>
    /// Pourcentage d'impayés
    /// </summary>
    public decimal PourcentageImpayes => TotalAttendu > 0
        ? Math.Round((SoldeGlobal / TotalAttendu) * 100, 2)
        : 0;

    /// <summary>
    /// Montant moyen par paiement (30 derniers jours)
    /// </summary>
    public decimal MontantMoyenPaiement => PaiementsRecents30Jours > 0
        ? Math.Round(PaiementsRecents30Jours / 30, 2)
        : 0;
};

/// <summary>
/// Répartition par type de frais
/// </summary>
public record RepartitionTypeFraisDto(
    string TypeFrais,
    decimal MontantTotal,
    decimal MontantEncaisse,
    decimal TauxRecouvrement
)
{
    /// <summary>
    /// Solde restant pour ce type de frais
    /// </summary>
    public decimal SoldeRestant => MontantTotal - MontantEncaisse;

    /// <summary>
    /// Pourcentage du total des frais
    /// </summary>
    public decimal? PourcentageDuTotal { get; init; }
}

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