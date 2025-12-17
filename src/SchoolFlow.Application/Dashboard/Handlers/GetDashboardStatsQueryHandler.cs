using System.Linq;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Dashboard.Queries;
using SchoolFlow.Domain.Entities;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Dashboard.Handlers;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken ct)
    {
        // 1. Stats de base
        var totalEleves = await _context.Eleves
            .Where(e => !e.IsArchived && e.Statut == StatutEleve.Actif)
            .CountAsync(ct);

        var totalFamilles = await _context.Familles
            .Where(f => !f.IsArchived)
            .CountAsync(ct);

         var totalFraisAttendus = await _context.Frais
            .Where(f => !f.IsArchived)
            .SumAsync(f => f.Montant, ct);

        var totalEncaisse = await _context.Frais
            .Where(f => !f.IsArchived)
            .SumAsync(f => f.MontantPaye, ct);

        var soldeGlobal = totalFraisAttendus - totalEncaisse;
        var tauxRecouvrement = totalFraisAttendus > 0
            ? Math.Round(totalEncaisse / totalFraisAttendus * 100, 2)
            : 0;

        // 2. Stats financières
        var statsFinancieres = await CalculerStatsFinancieres(ct);

        // 3. Alertes
        var alertes = await GenererAlertes(ct);

        // 4. Répartition par classe
        var classesData = await _context.Classes
            .Where(c => !c.IsArchived)
            .Select(c => new
            {
                c.Nom,
                // Charger les élèves avec leurs frais
                Eleves = c.Eleves
                    .Where(e => !e.IsArchived && e.Statut == StatutEleve.Actif)
                    .Select(e => new
                    {
                        TotalFrais = e.Frais.Where(f => !f.IsArchived).Sum(f => f.Montant),
                        TotalPaye = e.Frais.Where(f => !f.IsArchived).Sum(f => f.MontantPaye)
                    })
                    .ToList()
            })
            .ToListAsync(ct);
            
        //Calcil coté client pour eviter les problemes de traduction de requete Linq to SQL
        var statistiquesParClasses = classesData
            .Select(c =>
            {
                // Calculs intermédiaires
                var totalFraisClasse = c.Eleves.Sum(e => e.TotalFrais);
                var totalPayeClasse = c.Eleves.Sum(e => e.TotalPaye);
                var tauxRecouvrementClasse = totalFraisClasse > 0
                    ? Math.Round((totalPayeClasse / totalFraisClasse) * 100, 2)
                    : 0;

                return new StatistiqueClasseDto(
                    NomClasse: c.Nom,
                    NombreEleves: c.Eleves.Count,
                    TauxRecouvrement: tauxRecouvrementClasse
                );
            })
            .OrderBy(s => s.NomClasse)
            .ToList();


        // 5. Graphique évolution encaissements (6 derniers mois)
        var evolutionEncaissements = await GenererGraphiqueEvolution(ct);

        // 6. Construire le DTO avec le nouveau format
        var stats = new DashboardStatsDto(
            totalEleves,
            totalFamilles,
            totalFraisAttendus,
            totalEncaisse,
            soldeGlobal,
            tauxRecouvrement,
            statistiquesParClasses
        );

        return Result<DashboardStatsDto>.Success(stats);
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
            TotalFamillesImpayes: famillesImpayes,
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