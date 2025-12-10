using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;

namespace SchoolFlow.Application.Dashboard.Queries;

public record GetDashboardStatsQuery(Guid UtilisateurId, string Role) : IRequest<Result<DashboardStats>>;

public record DashboardStats
{
    public int NombreFamilles { get; init; }
    public int NombreEleves { get; init; }
    public decimal TotalEncaisse { get; init; }
    public decimal TotalAEncaisser { get; init; }
    public decimal TauxRecouvrement { get; init; }
    public int FamillesEnImpaye { get; init; }
    public List<RepartitionNiveau> RepartitionParNiveau { get; init; } = new();
}

public record RepartitionNiveau(string Niveau, int Effectif);

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStats>>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DashboardStats>> Handle(GetDashboardStatsQuery request, CancellationToken ct)
    {
        var familles = await _context.Familles
            .Include(f => f.Eleves)
            .ThenInclude(e => e.Frais)
            .Where(f => !f.IsArchived)
            .ToListAsync(ct);

        var eleves = await _context.Eleves
            .Include(e => e.Classe)
            .Where(e => !e.IsArchived && e.Statut == Domain.Entities.StatutEleve.Actif)
            .ToListAsync(ct);

        var totalEncaisse = familles.Sum(f => f.TotalPaye);
        var totalAEncaisser = familles.Sum(f => f.TotalDu);
        var tauxRecouvrement = totalAEncaisser > 0 
            ? (totalEncaisse / totalAEncaisser) * 100 
            : 0;

        var famillesEnImpaye = familles.Count(f => f.StatutPaiement == Domain.Entities.StatutPaiement.Impaye);

        var repartition = eleves
            .GroupBy(e => e.Classe.Niveau)
            .Select(g => new RepartitionNiveau(g.Key.ToString(), g.Count()))
            .OrderBy(r => r.Niveau)
            .ToList();

        var stats = new DashboardStats
        {
            NombreFamilles = familles.Count,
            NombreEleves = eleves.Count,
            TotalEncaisse = totalEncaisse,
            TotalAEncaisser = totalAEncaisser,
            TauxRecouvrement = Math.Round(tauxRecouvrement, 2),
            FamillesEnImpaye = famillesEnImpaye,
            RepartitionParNiveau = repartition
        };

        return Result<DashboardStats>.Success(stats);
    }
}