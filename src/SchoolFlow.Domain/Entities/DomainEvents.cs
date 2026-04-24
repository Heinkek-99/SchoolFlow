namespace SchoolFlow.Domain.Entities;

// ─── ÉCOLE ───────────────────────────────────────────────────────────────────

/// <summary>
/// Levé quand une école est créée → initialise les données par défaut.
/// </summary>
public record EcoleCreeeEvent(
    Guid EcoleId,
    string NomEcole,
    string CodeEcole,
    TypeEtablissement TypeEtablissement
) : DomainEvent;

/// <summary>
/// Levé quand une école passe au statut Active.
/// </summary>
public record EcoleValideeEvent(
    Guid EcoleId,
    string NomEcole
) : DomainEvent;

// ─── FAMILLE ─────────────────────────────────────────────────────────────────

/// <summary>
/// Levé lors de la création d'une famille.
/// → Peut déclencher : log d'audit, notification
/// </summary>
public record FamilleCreeeEvent(
    Guid FamilleId,
    Guid EcoleId,
    string NomFamille,
    string TelephonePrincipal
) : DomainEvent;

// ─── ÉLÈVE ───────────────────────────────────────────────────────────────────

/// <summary>
/// Levé lors de l'inscription d'un élève.
/// → DÉCLENCHE la génération automatique des frais standards.
/// C'est l'event le plus important du domaine.
/// </summary>
public record EleveInscritEvent(
    Guid EleveId,
    string Matricule,
    Guid FamilleId,
    Guid EcoleId,
    Guid ClasseId,
    Guid AnneeScolaireId
) : DomainEvent;

/// <summary>
/// Levé quand un élève est archivé.
/// </summary>
public record EleveArchiveEvent(
    Guid EleveId,
    string Matricule,
    Guid EcoleId,
    string Raison
) : DomainEvent;

// ─── PAIEMENT ────────────────────────────────────────────────────────────────

/// <summary>
/// Levé après enregistrement réussi d'un paiement.
/// → Peut déclencher : génération reçu PDF, alerte SMS, mise à jour tableau de bord
/// </summary>
public record PaiementEnregistreEvent(
    Guid PaiementId,
    string NumeroPaiement,
    Guid FamilleId,
    Guid EcoleId,
    decimal MontantTotal,
    DateTime DatePaiement,
    int NombreVentilations
) : DomainEvent;