using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Familles.Queries;
using SchoolFlow.Shared.Dtos;


public class SearchFamillesQueryHandler : IRequestHandler<SearchFamillesQuery, Result<List<FamilleDto>>>
{
    private readonly IApplicationDbContext _context;

    public SearchFamillesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<FamilleDto>>> Handle(SearchFamillesQuery request, CancellationToken ct)
    {
        var query = request.Query.ToLower();
        
        var familles = await _context.Familles
            .Include(f => f.Eleves.Where(e => !e.IsArchived))
                .ThenInclude(e => e.Frais.Where(fr => !fr.IsArchived))
            .Where(f => 
                f.NomPere.ToLower().Contains(query) ||
                (f.PrenomPere != null && f.PrenomPere!.ToLower().Contains(query)) ||
                f.TelephonePrincipal.Contains(query) ||
                (f.TelephonePere != null && f.TelephonePere!.Contains(query)))
            .OrderBy(f => f.NomPere)
            .Take(20)
            .AsNoTracking()
            .ToListAsync(ct);

        var result = familles.Select(f =>
        {
            var elevesActifs = f.Eleves.Where(e => !e.IsArchived).ToList();
            var totalDu = elevesActifs.Sum(e => e.Frais.Sum(fr => fr.Montant));
            var totalPaye = elevesActifs.Sum(e => e.Frais.Sum(fr => fr.MontantPaye));
            var soldeGlobal = totalDu - totalPaye;

            return new FamilleDto(
                f.Id,
                f.NomPere,
                f.PrenomPere,
                f.TelephonePrincipal,
                f.Ville,
                elevesActifs.Count,
                totalDu,
                totalPaye,
                soldeGlobal,
                CalculerStatutPaiement(totalDu, totalPaye)  
            );
        }).ToList();

        return Result<List<FamilleDto>>.Success(result);
    }
    private static string CalculerStatutPaiement(decimal totalDu, decimal totalPaye)
    {
        var solde = totalDu - totalPaye;
        if (solde <= 0) return "Payé";
        if (totalPaye > 0) return "Partiel";
        return "Impayé";
    }
}