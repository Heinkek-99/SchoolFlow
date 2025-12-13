using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Dashboard.Queries;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Dashboard.Handlers;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStats>>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DashboardStats>> Handle(GetDashboardStatsQuery request, CancellationToken ct)
    {
        // 1. Stats de base
        var nombreElevesActifs = await _context.Eleves
            .Where(e => !e.IsArchived && e.Statut == Domain.Entities.StatutEleve.Actif)
            .CountAsync(ct);

        var nombreFamilles = await _context.Familles
            .Where(f => !f.IsArchived)
            .CountAsync(ct);

        // 2. Stats financières
        var statsFinancieres = await CalculerStatsFinancieres(ct);

        // 3. Alertes
        var alertes = await GenererAlertes(ct);

        // 4. Répartition par classe
        var repartitionClasses = await _context.Classes
            .Where(c => !c.IsArchived)
            .Select(c => new StatistiqueClasseDto(
                c.Nom,
                c.Eleves.Count(e => !e.IsArchived && e.Statut == Domain.Entities.StatutEleve.Actif),
                c.Eleves.Any() && c.Eleves.Sum(e => e.Frais.Sum(f => f.Montant)) > 0
                    ? Math.Round((c.Eleves.Sum(e => e.Frais.Sum(f => f.MontantPaye)) / 
                                 c.Eleves.Sum(e => e.Frais.Sum(f => f.Montant))) * 100, 2)
                    : 0
            ))
            .OrderBy(s => s.NomClasse)
            .ToListAsync(ct);

        // 5. Graphique évolution encaissements (6 derniers mois)
        var evolutionEncaissements = await GenererGraphiqueEvolution(ct);

        // 6. Construire le DTO avec le nouveau format
        var dashboard = new DashboardStats(
            NombreElevesActifs: nombreElevesActifs,
            NombreFamilles: nombreFamilles,
            Finances: statsFinancieres,
            Alertes: alertes,
            RepartitionClasses: repartitionClasses,
            EvolutionEncaissements: evolutionEncaissements
        );

        return Result<DashboardStats>.Success(dashboard);
    }

    private async Task<StatistiquesFinancieres> CalculerStatsFinancieres(CancellationToken ct)
    {
        var totalAEncaisser = await _context.Frais
            .Where(f => !f.IsArchived)
            .SumAsync(f => f.Montant, ct);

        var totalEncaisse = await _context.Frais
            .Where(f => !f.IsArchived)
            .SumAsync(f => f.MontantPaye, ct);

        var soldeRestant = totalAEncaisser - totalEncaisse;
        var tauxRecouvrement = totalAEncaisser > 0 
            ? Math.Round((totalEncaisse / totalAEncaisser) * 100, 2) 
            : 0;

        var famillesImpayes = await _context.Familles
            .Where(f => !f.IsArchived && f.Eleves.Any(e => e.Frais.Sum(fr => fr.Montant - fr.MontantPaye) > 0))
            .CountAsync(ct);

        var montantMoyenImpaye = famillesImpayes > 0 
            ? Math.Round(soldeRestant / famillesImpayes, 2)
            : 0;

        return new StatistiquesFinancieres(
            TotalAEncaisser: totalAEncaisser,
            TotalEncaisse: totalEncaisse,
            SoldeRestant: soldeRestant,
            TauxRecouvrement: tauxRecouvrement,
            NombreFamillesImpayes: famillesImpayes,
            MontantMoyenImpaye: montantMoyenImpaye
        );
    }

    private async Task<List<AlerteDto>> GenererAlertes(CancellationToken ct)
    {
        var alertes = new List<AlerteDto>();

        // Alerte impayés critiques (> 90 jours)
        var impayesCritiques = await _context.Frais
            .Where(f => !f.IsArchived && 
                       (f.Montant - f.MontantPaye) > 0 && 
                       f.DateEcheance < DateTime.Now.AddDays(-90))
            .Select(f => f.Eleve.FamilleId)
            .Distinct()
            .CountAsync(ct);

        if (impayesCritiques > 0)
        {
            alertes.Add(new AlerteDto(
                Type: "ImpayesCritiques",
                Message: $"{impayesCritiques} famille(s) avec impayés de plus de 90 jours",
                Severite: "Critique",
                Date: DateTime.Now,
                EntityId: null
            ));
        }

        // Alerte échéances du mois
        var echeancesMois = await _context.Frais
            .Where(f => !f.IsArchived &&
                       (f.Montant - f.MontantPaye) > 0 &&
                       f.DateEcheance.Month == DateTime.Now.Month &&
                       f.DateEcheance.Year == DateTime.Now.Year)
            .SumAsync(f => f.Montant - f.MontantPaye, ct);

        if (echeancesMois > 0)
        {
            alertes.Add(new AlerteDto(
                Type: "EcheancesMois",
                Message: $"{echeancesMois:N0} FCFA d'échéances ce mois",
                Severite: "Information",
                Date: DateTime.Now,
                EntityId: null
            ));
        }

        return alertes;
    }

    private async Task<GraphiqueEvolutionDto> GenererGraphiqueEvolution(CancellationToken ct)
    {
        var labels = new List<string>();
        var encaissements = new List<decimal>();
        var objectifs = new List<decimal>();

        for (int i = 5; i >= 0; i--)
        {
            var mois = DateTime.Now.AddMonths(-i);
            labels.Add(mois.ToString("MMM yyyy"));

            var encaissementMois = await _context.Paiements
                .Where(p => !p.IsArchived &&
                           p.DatePaiement.Month == mois.Month && 
                           p.DatePaiement.Year == mois.Year)
                .SumAsync(p => p.MontantTotal, ct);

            encaissements.Add(encaissementMois);

            // Objectif fictif (à remplacer par config réelle)
            objectifs.Add(5_000_000);
        }

        return new GraphiqueEvolutionDto(
            Labels: labels,
            Encaissements: encaissements,
            Objectifs: objectifs
        );
    }
}