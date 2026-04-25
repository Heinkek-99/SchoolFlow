namespace SchoolFlow.Shared.Dtos;

// ========================
// PAIEMENT DTOs
// ========================

public record PaiementDto(
    Guid Id,
    string NumeroPaiement,
    Guid FamilleId,
    string NomFamille,
    decimal MontantTotal,
    DateTime DatePaiement,
    string ModePaiement,
    string? Reference,
    string? Commentaire,
    string NomUtilisateur,
    List<VentilationPaiementDto> Ventilations,
    DateTime CreatedAt
);

public record VentilationPaiementDto(
    Guid Id,
    Guid EleveId,
    string NomCompletEleve,
    string Matricule,
    decimal Montant,
    string? Remarque
);

public record PaiementListItemDto(
    Guid Id,
    string NumeroPaiement,
    string NomFamille,
    decimal MontantTotal,
    DateTime DatePaiement,
    string ModePaiement,
    int NombreEleves,
    string NomUtilisateur
);

public record VentilationProposeeDto(
    Guid EleveId,
    string EleveNom,
    Guid FraisId,
    string LibelleFrais,
    decimal MontantSuggere,
    decimal SoldeAvant,
    decimal SoldeApres
);

