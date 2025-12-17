using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Dashboard.Queries;

namespace SchoolFlow.Application.Dashboard.Handlers;

public class GetStatsFinancieresQueryHandler : IRequestHandler<GetStatsFinancieresQuery, Result<StatsFinancieresDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStatsFinancieresQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    // public async Task<Result<StatsFinancieresDetailDto>> Handle(GetStatsFinancieresQuery request, CancellationToken ct)

//     {
//         var query = _context.Frais.Where(f => !f.IsArchived);
//         var maintenant = DateTime.Now;
//         var debutMois = new DateTime(maintenant.Year, maintenant.Month, 1);
//         var debutTrimestre = debutMois.AddMonths(-(maintenant.Month - 1) % 3);
//         var debutAnnee = new DateTime(maintenant.Year, 1, 1);

//         // Encaissements périodes
//         var encaissementsMois = await _context.Paiements
//             .Where(p => !p.IsArchived && p.DatePaiement >= debutMois)
//             .SumAsync(p => p.MontantTotal, ct);

//         var encaissementsTrimestre = await _context.Paiements
//             .Where(p => !p.IsArchived && p.DatePaiement >= debutTrimestre)
//             .SumAsync(p => p.MontantTotal, ct);

//         var encaissementsAnnee = await _context.Paiements
//             .Where(p => !p.IsArchived && p.DatePaiement >= debutAnnee)
//             .SumAsync(p => p.MontantTotal, ct);

//         // Objectifs (fictifs - à remplacer par config)
//         var objectifMois = 5_000_000m;
//         var objectifAnnee = 60_000_000m;

//         var tauxRealisationMois = objectifMois > 0 
//             ? Math.Round((encaissementsMois / objectifMois) * 100, 2) 
//             : 0;
        
//         var tauxRealisationAnnee = objectifAnnee > 0 
//             ? Math.Round((encaissementsAnnee / objectifAnnee) * 100, 2) 
//             : 0;

//         // Familles impayés
//         var famillesImpayes = await _context.Familles
//             .Include(f => f.Eleves)
//                 .ThenInclude(e => e.Frais)
//             .Where(f => !f.IsArchived && f.Eleves.Any(e => e.Frais.Sum(fr => fr.Montant - fr.MontantPaye) > 0))
//             .Select(f => new FamilleImpayeDto(
//                 f.Id,
//                 f.NomFamille,
//                 f.TelephonePere ?? f.TelephoneMere ?? "N/A",
//                 f.Eleves.Count,
//                 f.Eleves.Sum(e => e.Frais.Sum(fr => fr.Montant)),
//                 f.Eleves.Sum(e => e.Frais.Sum(fr => fr.MontantPaye)),
//                 f.Eleves.Sum(e => e.Frais.Sum(fr => fr.Montant - fr.MontantPaye)),
//                 f.Eleves.SelectMany(e => e.Frais).Where(fr => fr.Montant - fr.MontantPaye > 0).Min(fr => fr.DateEcheance),
//                 (DateTime.Now - f.Eleves.SelectMany(e => e.Frais).Where(fr => fr.Montant - fr.MontantPaye > 0).Min(fr => fr.DateEcheance)).Days,
//                 "Normal"
//             ))
//             .ToListAsync(ct);

//         // Top 10 impayés
//         var top10Impayes = famillesImpayes
//             .OrderByDescending(f => f.SoldeRestant)
//             .Take(10)
//             .Select((f, index) => new TopImpayeDto(
//                 Rang: index + 1,
//                 NomFamille: f.NomFamille,
//                 MontantImpaye: f.SoldeRestant,
//                 NombreEnfants: f.NombreEnfants
//             ))
//             .ToList();

//         // Répartition modes paiement
//         var totalEncaissements = await _context.Paiements
//             .Where(p => !p.IsArchived)
//             .SumAsync(p => p.MontantTotal, ct);

//         var repartitionModes = await _context.Paiements
//             .Where(p => !p.IsArchived)
//             .GroupBy(p => p.ModePaiement)
//             .Select(g => new EncaissementParModeDto(
//                 g.Key.ToString(),
//                 g.Sum(p => p.MontantTotal),
//                 g.Count(),
//                 totalEncaissements > 0 
//                     ? Math.Round((g.Sum(p => p.MontantTotal) / totalEncaissements) * 100, 2) 
//                     : 0
//             ))
//             .OrderByDescending(x => x.Montant)
//             .ToListAsync(ct);

//         var stats = new StatsFinancieresDetailDto(
//             EncaissementsMois: encaissementsMois,
//             EncaissementsTrimestre: encaissementsTrimestre,
//             EncaissementsAnnee: encaissementsAnnee,
//             ObjectifMois: objectifMois,
//             ObjectifAnnee: objectifAnnee,
//             TauxRealisationMois: tauxRealisationMois,
//             TauxRealisationAnnee: tauxRealisationAnnee,
//             FamillesImpayes: famillesImpayes,
//             Top10Impayes: top10Impayes,
//             RepartitionModesPaiement: repartitionModes
//         );

//         return Result<StatsFinancieresDetailDto>.Success(stats);
//     }
// }

 public async Task<Result<StatsFinancieresDto>> Handle(GetStatsFinancieresQuery request, CancellationToken ct)
    {
        // Filtre de dates
        var query = _context.Frais.Where(f => !f.IsArchived);

        if (request.DateDebut.HasValue)
            query = query.Where(f => f.CreatedAt >= request.DateDebut.Value);

        if (request.DateFin.HasValue)
            query = query.Where(f => f.CreatedAt <= request.DateFin.Value);

        // Agrégations simples (une seule couche)
        var totalAttendu = await query.SumAsync(f => f.Montant, ct);
        var totalEncaisse = await query.SumAsync(f => f.MontantPaye, ct);
        var soldeGlobal = totalAttendu - totalEncaisse;
        var tauxRecouvrement = totalAttendu > 0
            ? Math.Round(totalEncaisse / totalAttendu * 100, 2)
            : 0;

        // Paiements récents (derniers 30 jours)
        var dateDebut30j = DateTime.Now.AddDays(-30);
        var paiementsRecents = await _context.Paiements
            .Where(p => !p.IsArchived && p.DatePaiement >= dateDebut30j)
            .SumAsync(p => p.MontantTotal, ct);

        // Répartition par type de frais (calcul simplifié)
        var repartitionParType = await query
            .Include(f => f.TypeFrais)
            .Select(f => new
            {
                TypeFraisNom = f.TypeFrais.Code,
                f.Montant,
                f.MontantPaye
            })
            .ToListAsync(ct); // ⚠️ Matérialisation

        var repartition = repartitionParType
            .GroupBy(f => f.TypeFraisNom)
            .Select(g => new RepartitionTypeFraisDto(
                g.Key,
                g.Sum(f => f.Montant),
                g.Sum(f => f.MontantPaye),
                g.Sum(f => f.Montant) > 0
                    ? Math.Round(g.Sum(f => f.MontantPaye) / g.Sum(f => f.Montant) * 100, 2)
                    : 0
            ))
            .OrderByDescending(r => r.MontantTotal)
            .ToList();

        var stats = new StatsFinancieresDto(
            totalAttendu,
            totalEncaisse,
            soldeGlobal,
            paiementsRecents,
            tauxRecouvrement,
            repartition
        );

        return Result<StatsFinancieresDto>.Success(stats);
    }
}