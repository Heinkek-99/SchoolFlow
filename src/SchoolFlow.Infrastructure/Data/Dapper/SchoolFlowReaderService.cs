namespace SchoolFlow.Infrastructure.Data.Dapper;
 
using global::Dapper;
using Npgsql;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Shared.Dtos;
 
public class SchoolFlowReadService : ISchoolFlowReadService
{
    private readonly string _connectionString;
 
    public SchoolFlowReadService(string connectionString)
    {
        _connectionString = connectionString;
    }
 
    private NpgsqlConnection CreateConnection() => new(_connectionString);
 
    // ─── DASHBOARD ──────────────────────────────────────────────────────────
 
    public async Task<DashboardStatsReadDto> GetDashboardStatsAsync(
        Guid ecoleId,
        CancellationToken ct = default)
    {
        await using var conn = CreateConnection();
 
        // 1 requête SQL au lieu de 5 appels EF Core séparés
        const string sql = """
            SELECT
                (SELECT COUNT(*)
                 FROM "Eleves" e
                 WHERE e."EcoleId" = @ecoleId
                   AND e."IsArchived" = false
                   AND e."Statut" = 'Actif') AS "NombreElevesActifs",
 
                (SELECT COUNT(*)
                 FROM "Familles" f
                 WHERE f."EcoleId" = @ecoleId
                   AND f."IsArchived" = false) AS "NombreFamilles",
 
                (SELECT COUNT(*)
                 FROM "Classes" c
                 WHERE c."EcoleId" = @ecoleId
                   AND c."IsArchived" = false) AS "NombreClasses",
 
                (SELECT COALESCE(SUM(fr."Montant"), 0)
                 FROM "Frais" fr
                 INNER JOIN "Eleves" el ON el."Id" = fr."EleveId"
                 WHERE el."EcoleId" = @ecoleId
                   AND fr."IsArchived" = false) AS "TotalFraisDus",
 
                (SELECT COALESCE(SUM(fr."MontantPaye"), 0)
                 FROM "Frais" fr
                 INNER JOIN "Eleves" el ON el."Id" = fr."EleveId"
                 WHERE el."EcoleId" = @ecoleId
                   AND fr."IsArchived" = false) AS "TotalPaye",
 
                (SELECT COUNT(DISTINCT fa."Id")
                 FROM "Familles" fa
                 INNER JOIN "Eleves" el ON el."FamilleId" = fa."Id"
                 INNER JOIN "Frais" fr ON fr."EleveId" = el."Id"
                 WHERE fa."EcoleId" = @ecoleId
                   AND fa."IsArchived" = false
                   AND fr."IsArchived" = false
                   AND fr."Montant" > fr."MontantPaye") AS "NombreFamillesImpayees"
            """;
 
        var stats = await conn.QuerySingleAsync<DashboardStatsReadDto>(sql, new { ecoleId });
 
        stats.SoldeRestant = stats.TotalFraisDus - stats.TotalPaye;
        stats.TauxRecouvrement = stats.TotalFraisDus > 0
            ? Math.Round((stats.TotalPaye / stats.TotalFraisDus) * 100, 2)
            : 0;
 
        return stats;
        
    }
 
    // ─── FAMILLES ───────────────────────────────────────────────────────────
 
    public async Task<PagedResultDto<FamilleDto>> GetFamillesAsync(
        Guid ecoleId,
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        await using var conn = CreateConnection();
 
        var offset = (page - 1) * pageSize;
        var searchParam = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";
 
        const string sql = """
            SELECT
                fa."Id",
                fa."NomPere",
                fa."PrenomPere",
                fa."NomMere",
                fa."PrenomMere",
                fa."TelephonePrincipal",
                fa."Ville",
                fa."QuartierCommune",
                COUNT(DISTINCT el."Id") AS "NombreEnfants",
                COALESCE(SUM(fr."Montant"), 0) AS "TotalDu",
                COALESCE(SUM(fr."MontantPaye"), 0) AS "TotalPaye",
                COALESCE(SUM(fr."Montant" - fr."MontantPaye"), 0) AS "SoldeGlobal"
            FROM "Familles" fa
            LEFT JOIN "Eleves" el ON el."FamilleId" = fa."Id" AND el."IsArchived" = false
            LEFT JOIN "Frais" fr  ON fr."EleveId" = el."Id"   AND fr."IsArchived" = false
            WHERE fa."EcoleId" = @ecoleId
              AND fa."IsArchived" = false
              AND (@search::text IS NULL
                   OR fa."NomPere"            ILIKE @search
                   OR fa."NomMere"            ILIKE @search
                   OR fa."TelephonePrincipal" ILIKE @search)
            GROUP BY fa."Id", fa."NomPere", fa."PrenomPere", fa."NomMere",
                     fa."PrenomMere", fa."TelephonePrincipal", fa."Ville", fa."QuartierCommune"
            ORDER BY fa."NomPere"
            LIMIT @pageSize OFFSET @offset
            """;
 
        const string countSql = """
            SELECT COUNT(*)
            FROM "Familles" fa
            WHERE fa."EcoleId" = @ecoleId
              AND fa."IsArchived" = false
              AND (@search::text IS NULL
                   OR fa."NomPere"            ILIKE @search
                   OR fa."NomMere"            ILIKE @search
                   OR fa."TelephonePrincipal" ILIKE @search)
            """;
 
        var p = new { ecoleId, search = searchParam, pageSize, offset };
 
        var items = (await conn.QueryAsync<FamilleDto>(sql, p)).ToList();
        var total = await conn.QuerySingleAsync<int>(countSql, p);
 
        return new PagedResultDto<FamilleDto>(items, total, page, pageSize);
    }
 
    // ─── HISTORIQUE PAIEMENTS ────────────────────────────────────────────────
 
    public async Task<IReadOnlyList<PaiementDto>> GetHistoriquePaiementsFamilleAsync(
        Guid ecoleId,
        Guid familleId,
        CancellationToken ct = default)
    {
        await using var conn = CreateConnection();
 
        const string sql = """
            SELECT
                p."Id",
                p."NumeroPaiement",
                p."MontantTotal",
                p."DatePaiement",
                p."ModePaiement"::text AS "ModePaiement",
                p."Reference",
                p."Commentaire",
                u."Prenom" || ' ' || u."Nom" AS "EnregistrePar",
                COUNT(vp."Id")               AS "NombreVentilations"
            FROM "Paiements" p
            LEFT JOIN "Utilisateurs" u   ON u."Id" = p."EnregistrePar"
            LEFT JOIN "VentilationsPaiement" vp ON vp."PaiementId" = p."Id"
            WHERE p."FamilleId" = @familleId
              AND p."EcoleId" = @ecoleId
              AND p."IsArchived" = false
            GROUP BY p."Id", p."NumeroPaiement", p."MontantTotal", p."DatePaiement",
                     p."ModePaiement", p."Reference", p."Commentaire", u."Prenom", u."Nom"
            ORDER BY p."DatePaiement" DESC
            """;
 
        var result = await conn.QueryAsync<PaiementDto>(sql, new { ecoleId, familleId });
        return result.ToList().AsReadOnly();
    }
 
    // ─── TOP IMPAYÉS ─────────────────────────────────────────────────────────
 
    public async Task<IReadOnlyList<FamilleImpayeeDto>> GetTopImpayesAsync(
        Guid ecoleId,
        int top = 10,
        CancellationToken ct = default)
    {
        await using var conn = CreateConnection();
 
        const string sql = """
            SELECT
                fa."Id"                                           AS "FamilleId",
                fa."NomPere" || ' ' || COALESCE(fa."PrenomPere", '') AS "NomFamille",
                fa."TelephonePrincipal",
                COUNT(DISTINCT el."Id")                           AS "NombreEnfants",
                SUM(fr."Montant" - fr."MontantPaye")              AS "MontantImpoye",
                MAX(p."DatePaiement")                             AS "DateDernierPaiement"
            FROM "Familles" fa
            INNER JOIN "Eleves" el ON el."FamilleId" = fa."Id" AND el."IsArchived" = false
            INNER JOIN "Frais" fr  ON fr."EleveId" = el."Id"  AND fr."IsArchived" = false
                                   AND fr."Montant" > fr."MontantPaye"
            LEFT JOIN "Paiements" p ON p."FamilleId" = fa."Id" AND p."IsArchived" = false
            WHERE fa."EcoleId" = @ecoleId AND fa."IsArchived" = false
            GROUP BY fa."Id", fa."NomPere", fa."PrenomPere", fa."TelephonePrincipal"
            ORDER BY "MontantImpoye" DESC
            LIMIT @top
            """;
 
        var result = await conn.QueryAsync<FamilleImpayeeDto>(sql, new { ecoleId, top });
        return result.ToList().AsReadOnly();
    }
 
    // ─── ÉLÈVES PAR CLASSE ───────────────────────────────────────────────────
 
    public async Task<IReadOnlyList<EleveListDto>> GetElevesParClasseAsync(
        Guid ecoleId,
        Guid classeId,
        CancellationToken ct = default)
    {
        await using var conn = CreateConnection();
 
        const string sql = """
            SELECT
                e."Id",
                e."Matricule",
                e."Nom",
                e."Prenom",
                e."Sexe"::text    AS "Sexe",
                e."DateNaissance",
                e."PhotoPath",
                e."Statut"::text  AS "Statut",
                c."Nom"           AS "NomClasse",
                fa."NomPere",
                fa."TelephonePrincipal" AS "TelephoneFamille",
                COALESCE(SUM(fr."Montant"), 0)      AS "TotalDu",
                COALESCE(SUM(fr."MontantPaye"), 0)  AS "TotalPaye",
                COALESCE(SUM(fr."Montant" - fr."MontantPaye"), 0) AS "Solde"
            FROM "Eleves" e
            INNER JOIN "Classes" c   ON c."Id" = e."ClasseId"
            INNER JOIN "Familles" fa ON fa."Id" = e."FamilleId"
            LEFT JOIN "Frais" fr     ON fr."EleveId" = e."Id" AND fr."IsArchived" = false
            WHERE e."EcoleId" = @ecoleId
              AND e."ClasseId" = @classeId
              AND e."IsArchived" = false
            GROUP BY e."Id", e."Matricule", e."Nom", e."Prenom", e."Sexe",
                     e."DateNaissance", e."PhotoPath", e."Statut",
                     c."Nom", fa."NomPere", fa."TelephonePrincipal"
            ORDER BY e."Nom", e."Prenom"
            """;
 
        var result = await conn.QueryAsync<EleveListDto>(sql, new { ecoleId, classeId });
        return result.ToList().AsReadOnly();
    }

    // ─── BULLETINS PAR CLASSE ────────────────────────────────────────────────────

    public async Task<IReadOnlyList<BulletinResumeDto>> GetBulletinsClasseAsync(
        Guid ecoleId,
        Guid classeId,
        Guid periodeId,
        CancellationToken ct = default)
    {
        await using var conn = CreateConnection();

        const string sql = """
            SELECT
                b."Id",
                e."Nom" || ' ' || e."Prenom"  AS "EleveNom",
                e."Matricule"                  AS "EleveMatricule",
                b."MoyenneGenerale",
                b."RangClasse",
                b."EffectifClasse",
                CASE
                    WHEN b."MoyenneGenerale" >= 16 THEN 'Très Bien'
                    WHEN b."MoyenneGenerale" >= 14 THEN 'Bien'
                    WHEN b."MoyenneGenerale" >= 12 THEN 'Assez Bien'
                    WHEN b."MoyenneGenerale" >= 10 THEN 'Passable'
                    ELSE 'Insuffisant'
                END AS "Appreciation",
                b."EstPublie"
            FROM "Bulletins" b
            INNER JOIN "Eleves" e ON e."Id" = b."EleveId"
            WHERE b."EcoleId"   = @ecoleId
              AND e."ClasseId"  = @classeId
              AND b."PeriodeId" = @periodeId
              AND b."IsArchived" = false
              AND e."IsArchived" = false
            ORDER BY b."RangClasse"
            """;

        var rows = await conn.QueryAsync<BulletinRow>(sql, new { ecoleId, classeId, periodeId });
        return rows.Select(r => new BulletinResumeDto(
            r.Id, r.EleveNom, r.EleveMatricule,
            r.MoyenneGenerale, r.RangClasse, r.EffectifClasse,
            r.Appreciation, r.EstPublie
        )).ToList().AsReadOnly();
    }

    // ─── EMPLOI DU TEMPS PAR CLASSE ──────────────────────────────────────────────

    public async Task<EmploiDuTempsClasseDto> GetEmploiDuTempsClasseAsync(
        Guid ecoleId,
        Guid classeId,
        Guid anneeScolaireId,
        CancellationToken ct = default)
    {
        await using var conn = CreateConnection();

        const string sql = """
            SELECT
                ch."Id",
                ch."Jour"                               AS "Jour",
                TO_CHAR(ch."HeureDebut", 'HH24:MI')     AS "HeureDebut",
                TO_CHAR(ch."HeureFin",   'HH24:MI')     AS "HeureFin",
                m."Libelle"                             AS "Matiere",
                CASE WHEN en."Id" IS NOT NULL
                     THEN u."Prenom" || ' ' || u."Nom"
                     ELSE NULL END                      AS "Enseignant",
                ch."Salle",
                c."Nom"                                 AS "NomClasse"
            FROM "CreneauxHoraires" ch
            INNER JOIN "Matieres" m   ON m."Id"  = ch."MatiereId"   AND m."IsArchived"  = false
            INNER JOIN "Classes"  c   ON c."Id"  = ch."ClasseId"
            LEFT  JOIN "Enseignants" en ON en."Id" = ch."EnseignantId" AND en."IsArchived" = false
            LEFT  JOIN "Utilisateurs" u ON u."Id"  = en."UtilisateurId" AND u."IsArchived" = false
            WHERE ch."EcoleId"         = @ecoleId
              AND ch."ClasseId"        = @classeId
              AND ch."AnneeScolaireId" = @anneeScolaireId
              AND ch."IsArchived"      = false
            ORDER BY ch."Jour", ch."HeureDebut"
            """;

        var rows = (await conn.QueryAsync<CreneauRow>(sql,
            new { ecoleId, classeId, anneeScolaireId })).ToList();

        var nomClasse = rows.FirstOrDefault()?.NomClasse ?? string.Empty;

        var planningParJour = rows
            .GroupBy(r => r.Jour)
            .ToDictionary(
                g => g.Key,
                g => g.Select(r => new CreneauDto(
                    r.Id, r.Jour, r.HeureDebut, r.HeureFin,
                    r.Matiere, r.Enseignant, r.Salle
                )).ToList()
            );

        return new EmploiDuTempsClasseDto(classeId, nomClasse, planningParJour);
    }

    // ─── HELPERS PRIVÉS ──────────────────────────────────────────────────────────

    private class BulletinRow
    {
        public Guid    Id              { get; set; }
        public string  EleveNom        { get; set; } = string.Empty;
        public string  EleveMatricule  { get; set; } = string.Empty;
        public decimal MoyenneGenerale { get; set; }
        public int     RangClasse      { get; set; }
        public int     EffectifClasse  { get; set; }
        public string  Appreciation    { get; set; } = string.Empty;
        public bool    EstPublie       { get; set; }
    }

    private class CreneauRow
    {
        public Guid    Id         { get; set; }
        public string  Jour       { get; set; } = string.Empty;
        public string  HeureDebut { get; set; } = string.Empty;
        public string  HeureFin   { get; set; } = string.Empty;
        public string  Matiere    { get; set; } = string.Empty;
        public string? Enseignant { get; set; }
        public string? Salle      { get; set; }
        public string  NomClasse  { get; set; } = string.Empty;
    }
}