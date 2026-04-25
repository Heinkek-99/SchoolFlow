namespace SchoolFlow.Shared.Dtos;

public record EleveDossierDto(
    Guid Id,
    string Matricule,
    string Nom,
    string Prenom,
    string NomComplet,
    DateTime DateNaissance,
    string LieuNaissance,
    string Sexe,
    string? PhotoPath,
    string? Nationalite,
    string? GroupeSanguin,
    string? ContactUrgence,
    string Statut,
    // Classe
    string? NomClasse,
    string? SousSysteme,
    string? Section,
    // Famille
    Guid FamilleId,
    string NomFamille,
    string TelephoneFamille,
    // Finances
    decimal TotalDu,
    decimal TotalPaye,
    decimal Solde,
    decimal TauxPaiement,
    List<FraisDto> Frais,
    List<PaiementDetailDto> DerniersPaiements
);