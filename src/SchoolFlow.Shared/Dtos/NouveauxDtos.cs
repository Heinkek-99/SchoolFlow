// ============================================================
// FICHIER : src/SchoolFlow.Shared/Dtos/NouveauxDtos.cs
// ACTION  : Fichier à CRÉER (regroupe les DTOs manquants)
// ============================================================

namespace SchoolFlow.Shared.Dtos;

// ─── PAGINATION ──────────────────────────────────────────────────────────────

/// <summary>
/// Résultat paginé générique — utilisé par Dapper et les Query Handlers.
/// </summary>
public class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public PagedResultDto(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}

// ─── ÉCOLES ──────────────────────────────────────────────────────────────────

public record EcoleDetailDto(
    Guid Id,
    string Nom,
    string CodeEcole,
    string Type,
    string Statut,
    string Adresse,
    string Ville,
    string? Quartier,
    string? Region,
    string Pays,
    string TelephonePrincipal,
    string? Email,
    string? SiteWeb,
    string? Slogan,
    string? LogoPath,
    string NomDirecteur,
    string PrenomDirecteur,
    string? TelephoneDirecteur,
    string? EmailDirecteur,
    DateTime CreatedAt,
    DateTime? DateValidation,
    int NombreUtilisateurs,
    int NombreEleves
);

public record EcoleListItemDto(
    Guid Id,
    string Nom,
    string CodeEcole,
    string Type,
    string Statut,
    string Ville,
    string Pays,
    string NomDirecteur,
    string TelephonePrincipal,
    DateTime CreatedAt
);

// ─── DASHBOARD ───────────────────────────────────────────────────────────────

/// <summary>
/// DTO Dashboard — compatible Dapper (classe avec setters, pas record).
/// Dapper ne supporte pas les records avec constructor-only init.
/// </summary>
public class DashboardStatsReadDto
{
    public int NombreElevesActifs { get; set; }
    public int NombreFamilles { get; set; }
    public int NombreClasses { get; set; }
    public decimal TotalFraisDus { get; set; }
    public decimal TotalPaye { get; set; }
    public decimal SoldeRestant { get; set; }
    public decimal TauxRecouvrement { get; set; }
    public int NombreFamillesImpayees { get; set; }
}

// ─── FAMILLES ────────────────────────────────────────────────────────────────

/// <summary>
/// DTO Famille liste — compatible Dapper.
/// </summary>
public class FamilleListDto
{
    public Guid Id { get; set; }
    public string NomPere { get; set; } = string.Empty;
    public string? PrenomPere { get; set; }
    public string? NomMere { get; set; }
    public string? PrenomMere { get; set; }
    public string TelephonePrincipal { get; set; } = string.Empty;
    public string Ville { get; set; } = string.Empty;
    public string? QuartierCommune { get; set; }
    public int NombreEnfants { get; set; }
    public decimal TotalDu { get; set; }
    public decimal TotalPaye { get; set; }
    public decimal SoldeGlobal { get; set; }

    public string NomFamille => !string.IsNullOrWhiteSpace(NomPere)
        ? $"{NomPere} {PrenomPere}".Trim()
        : NomMere ?? "Famille";

    public string StatutPaiement => SoldeGlobal <= 0 ? "Payé"
        : TotalPaye > 0 ? "Partiel" : "Impayé";
}

// ─── IMPAYÉS ─────────────────────────────────────────────────────────────────

/// <summary>
/// DTO Top impayés — compatible Dapper.
/// </summary>
public class FamilleImpayeeDto
{
    public Guid FamilleId { get; set; }
    public string NomFamille { get; set; } = string.Empty;
    public string TelephonePrincipal { get; set; } = string.Empty;
    public int NombreEnfants { get; set; }
    public decimal MontantImpoye { get; set; }
    public DateTime? DateDernierPaiement { get; set; }

    public string NiveauUrgence => MontantImpoye switch
    {
        > 500_000 => "Critique",
        > 200_000 => "Urgent",
        > 50_000  => "Attention",
        _          => "Normal"
    };
}

// ─── PAIEMENTS ────────────────────────────────────────────────────────────────

/// <summary>
/// DTO Paiement liste — compatible Dapper.
/// </summary>
public class PaiementListDto
{
    public Guid Id { get; set; }
    public string NumeroPaiement { get; set; } = string.Empty;
    public decimal MontantTotal { get; set; }
    public DateTime DatePaiement { get; set; }
    public string ModePaiement { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? Commentaire { get; set; }
    public string EnregistrePar { get; set; } = string.Empty;
    public int NombreVentilations { get; set; }
}

// ─── ÉLÈVES ───────────────────────────────────────────────────────────────────

/// <summary>
/// DTO Élève liste — compatible Dapper.
/// </summary>
public class EleveListDto
{
    public Guid Id { get; set; }
    public string Matricule { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Sexe { get; set; } = string.Empty;
    public DateTime DateNaissance { get; set; }
    public string? PhotoPath { get; set; }
    public string Statut { get; set; } = string.Empty;
    public string NomClasse { get; set; } = string.Empty;
    public string NomPere { get; set; } = string.Empty;
    public string TelephoneFamille { get; set; } = string.Empty;
    public decimal TotalDu { get; set; }
    public decimal TotalPaye { get; set; }
    public decimal Solde { get; set; }

    public string NomComplet => $"{Prenom} {Nom}";
}