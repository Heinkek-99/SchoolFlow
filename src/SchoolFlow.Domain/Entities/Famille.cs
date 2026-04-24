// ============================================================
// FICHIER : src/SchoolFlow.Domain/Entities/Famille.cs
// ACTION  : Remplace entièrement le fichier existant
// CHANGEMENTS :
//   - Hérite de TenantEntity (ajout EcoleId)
//   - Propriétés private set (encapsulation DDD)
//   - Factory method Creer() qui lève un DomainEvent
//   - Méthodes métier Archiver(), MettreAJour() avec guards
// ============================================================

namespace SchoolFlow.Domain.Entities;

public class Famille : TenantEntity  // ← était BaseEntity
{
    // Père
    public string NomPere { get; private set; } = string.Empty;
    public string? PrenomPere { get; private set; }
    public string? TelephonePere { get; private set; }
    public string? EmailPere { get; private set; }
    public string? ProfessionPere { get; private set; }

    // Mère
    public string? NomMere { get; private set; }
    public string? PrenomMere { get; private set; }
    public string? TelephoneMere { get; private set; }
    public string? EmailMere { get; private set; }
    public string? ProfessionMere { get; private set; }

    // Adresse
    public string Adresse { get; private set; } = string.Empty;
    public string Ville { get; private set; } = string.Empty;
    public string? QuartierCommune { get; private set; }
    public string TelephonePrincipal { get; private set; } = string.Empty;
    public string? TelephoneSecondaire { get; private set; }

    // Navigation
    public ICollection<Eleve> Eleves { get; private set; } = new List<Eleve>();
    public ICollection<Paiement> Paiements { get; private set; } = new List<Paiement>();

    // ─── PROPRIÉTÉS CALCULÉES ────────────────────────────────────────────────

    public string NomFamille =>
        !string.IsNullOrWhiteSpace(NomPere)
            ? $"{NomPere} {PrenomPere}".Trim()
            : NomMere ?? "Famille";

    public decimal TotalDu => Eleves.Sum(e => e.TotalDu);
    public decimal TotalPaye => Eleves.Sum(e => e.TotalPaye);
    public decimal SoldeGlobal => TotalDu - TotalPaye;

    public decimal TauxRecouvrement =>
        TotalDu > 0 ? Math.Round((TotalPaye / TotalDu) * 100, 2) : 0;

    public StatutPaiement StatutPaiement =>
        SoldeGlobal <= 0 ? StatutPaiement.Paye
        : TotalPaye > 0 ? StatutPaiement.Partiel
        : StatutPaiement.Impaye;

    public int NombreEnfantsActifs =>
        Eleves.Count(e => !e.IsArchived && e.Statut == StatutEleve.Actif);

    public DateTime? DateDernierPaiement =>
        Paiements.Any() ? Paiements.Max(p => p.DatePaiement) : null;

    // ─── FACTORY METHOD ──────────────────────────────────────────────────────

    public static Famille Creer(
        Guid ecoleId,
        string nomPere,
        string? prenomPere,
        string? telephonePere,
        string? emailPere,
        string? professionPere,
        string? nomMere,
        string? prenomMere,
        string? telephoneMere,
        string adresse,
        string ville,
        string? quartierCommune,
        string telephonePrincipal,
        string? telephoneSecondaire = null)
    {
        if (string.IsNullOrWhiteSpace(nomPere) && string.IsNullOrWhiteSpace(nomMere))
            throw new ArgumentException("Au moins un parent (père ou mère) est obligatoire.");

        var famille = new Famille
        {
            EcoleId = ecoleId,
            NomPere = nomPere ?? string.Empty,
            PrenomPere = prenomPere,
            TelephonePere = telephonePere,
            EmailPere = emailPere,
            ProfessionPere = professionPere,
            NomMere = nomMere,
            PrenomMere = prenomMere,
            TelephoneMere = telephoneMere,
            Adresse = adresse,
            Ville = ville,
            QuartierCommune = quartierCommune,
            TelephonePrincipal = telephonePrincipal,
            TelephoneSecondaire = telephoneSecondaire
        };

        famille.RaiseDomainEvent(new FamilleCreeeEvent(
            famille.Id, ecoleId, famille.NomFamille, telephonePrincipal));

        return famille;
    }

    // ─── MÉTHODES MÉTIER ─────────────────────────────────────────────────────

    public void MettreAJour(
        string nomPere, string? prenomPere, string? telephonePere, string? emailPere,
        string? nomMere, string? prenomMere, string? telephoneMere,
        string adresse, string ville, string? quartier, string telephonePrincipal)
    {
        NomPere = nomPere;
        PrenomPere = prenomPere;
        TelephonePere = telephonePere;
        EmailPere = emailPere;
        NomMere = nomMere;
        PrenomMere = prenomMere;
        TelephoneMere = telephoneMere;
        Adresse = adresse;
        Ville = ville;
        QuartierCommune = quartier;
        TelephonePrincipal = telephonePrincipal;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archiver(Guid archivedBy, string raison)
    {
        if (NombreEnfantsActifs > 0)
            throw new InvalidOperationException(
                $"Impossible d'archiver : {NombreEnfantsActifs} enfant(s) actif(s).");

        IsArchived = true;
        ArchivedAt = DateTime.UtcNow;
        ArchivedBy = archivedBy;
        ArchiveReason = raison;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum StatutPaiement { Paye = 1, Partiel = 2, Impaye = 3 }