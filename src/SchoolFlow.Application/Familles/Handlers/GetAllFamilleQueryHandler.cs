using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Familles.Queries;
using SchoolFlow.Shared.Dtos;

public class GetAllFamillesQueryHandler : IRequestHandler<GetAllFamillesQuery, Result<List<FamilleDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllFamillesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<FamilleDto>>> Handle(GetAllFamillesQuery request, CancellationToken ct)
    {
        var familles = await _context.Familles
            .Include(f => f.Eleves.Where(e => !e.IsArchived))
                .ThenInclude(e => e.Frais.Where(fr => !fr.IsArchived))
            .OrderBy(f => f.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .AsNoTracking()
            .ToListAsync(ct);

            var result = familles.Select(f =>
            {
                var elevesActifs = f.Eleves.Where(e => !e.IsArchived).ToList();
                // var nombreEnfants = f.Eleves.Count(e => !e.IsArchived);
                var totalDu = elevesActifs.Sum(e => e.Frais.Sum(fr => fr.Montant));
                var totalPaye = elevesActifs.Sum(e => e.Frais.Sum(fr => fr.MontantPaye));
                var soldeGlobal = totalDu - totalPaye;
                var statutPaiement = CalculerStatut(totalDu, totalPaye);

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
                    CalculerStatut(totalDu, totalPaye)  
                );
            }).ToList();

        return Result<List<FamilleDto>>.Success(result);
    }

    private static string CalculerStatut(decimal totalDu, decimal totalPaye)
    {
        var solde = totalDu - totalPaye;
        if (solde <= 0) return "Payé";
        if (totalPaye > 0) return "Partiel";
        return "Impayé";
    }
}