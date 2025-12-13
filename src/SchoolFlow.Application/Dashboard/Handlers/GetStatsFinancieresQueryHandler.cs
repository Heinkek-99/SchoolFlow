using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Dashboard.Queries;

namespace SchoolFlow.Application.Dashboard.Handlers;

public class GetStatsFinancieresQueryHandler : IRequestHandler<GetStatsFinancieresQuery, Result<StatsFinancieresDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStatsFinancieresQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StatsFinancieresDetailDto>> Handle(GetStatsFinancieresQuery request, CancellationToken ct)
    {
        var maintenant = DateTime.Now;
        var debutMois = new DateTime(maintenant.Year, maintenant.Month, 1);
        var debutTrimestre = debutMois.AddMonths(-(maintenant.Month - 1) % 3);
        var debutAnnee = new DateTime(maintenant.Year, 1, 1);

        // Encaissements périodes
        var encaissementsMois = await _context.Paiements
            .Where(p => !p.IsArchived && p.DatePaiement >= debutMois)
            .SumAsync(p => p.MontantTotal, ct);

        var encaissementsTrimestre = await _context.Paiements
            .Where(p => !p.IsArchived && p.DatePaiement >= debutTrimestre)
            .SumAsync(p => p.MontantTotal, ct);

        var encaissementsAnnee = await _context.Paiements
            .Where(p => !p.IsArchived && p.DatePaiement >= debutAnnee)
            .SumAsync(p => p.MontantTotal, ct);

        // Objectifs (fictifs - à remplacer par config)
        var objectifMois = 5_000_000m;
        var objectifAnnee = 60_000_000m;

        var tauxRealisationMois = objectifMois > 0 
            ? Math.Round((encaissementsMois / objectifMois) * 100, 2) 
            : 0;
        
        var tauxRealisationAnnee = objectifAnnee > 0 
            ? Math.Round((encaissementsAnnee / objectifAnnee) * 100, 2) 
            : 0;

        // Familles impayés
        var famillesImpayes = await _context.Familles
            .Include(f => f.Eleves)
                .ThenInclude(e => e.Frais)
            .Where(f => !f.IsArchived && f.Eleves.Any(e => e.Frais.Sum(fr => fr.Montant - fr.MontantPaye) > 0))
            .Select(f => new FamilleImpayeDto(
                f.Id,
                f.NomFamille,
                f.TelephonePere ?? f.TelephoneMere ?? "N/A",
                f.Eleves.Count,
                f.Eleves.Sum(e => e.Frais.Sum(fr => fr.Montant)),
                f.Eleves.Sum(e => e.Frais.Sum(fr => fr.MontantPaye)),
                f.Eleves.Sum(e => e.Frais.Sum(fr => fr.Montant - fr.MontantPaye)),
                f.Eleves.SelectMany(e => e.Frais).Where(fr => fr.Montant - fr.MontantPaye > 0).Min(fr => fr.DateEcheance),
                (DateTime.Now - f.Eleves.SelectMany(e => e.Frais).Where(fr => fr.Montant - fr.MontantPaye > 0).Min(fr => fr.DateEcheance)).Days,
                "Normal"
            ))
            .ToListAsync(ct);

        // Top 10 impayés
        var top10Impayes = famillesImpayes
            .OrderByDescending(f => f.SoldeRestant)
            .Take(10)
            .Select((f, index) => new TopImpayeDto(
                Rang: index + 1,
                NomFamille: f.NomFamille,
                MontantImpaye: f.SoldeRestant,
                NombreEnfants: f.NombreEnfants
            ))
            .ToList();

        // Répartition modes paiement
        var totalEncaissements = await _context.Paiements
            .Where(p => !p.IsArchived)
            .SumAsync(p => p.MontantTotal, ct);

        var repartitionModes = await _context.Paiements
            .Where(p => !p.IsArchived)
            .GroupBy(p => p.ModePaiement)
            .Select(g => new EncaissementParModeDto(
                g.Key.ToString(),
                g.Sum(p => p.MontantTotal),
                g.Count(),
                totalEncaissements > 0 
                    ? Math.Round((g.Sum(p => p.MontantTotal) / totalEncaissements) * 100, 2) 
                    : 0
            ))
            .OrderByDescending(x => x.Montant)
            .ToListAsync(ct);

        var stats = new StatsFinancieresDetailDto(
            EncaissementsMois: encaissementsMois,
            EncaissementsTrimestre: encaissementsTrimestre,
            EncaissementsAnnee: encaissementsAnnee,
            ObjectifMois: objectifMois,
            ObjectifAnnee: objectifAnnee,
            TauxRealisationMois: tauxRealisationMois,
            TauxRealisationAnnee: tauxRealisationAnnee,
            FamillesImpayes: famillesImpayes,
            Top10Impayes: top10Impayes,
            RepartitionModesPaiement: repartitionModes
        );

        return Result<StatsFinancieresDetailDto>.Success(stats);
    }
}