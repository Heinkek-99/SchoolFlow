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
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(f => new FamilleDto(
                f.Id,
                f.NomPere,
                f.PrenomPere,
                f.TelephonePrincipal,
                f.Ville,
                f.Eleves.Count(e => !e.IsArchived),
                f.Eleves.Sum(e => e.Frais.Sum(fr => fr.Montant)),
                f.Eleves.Sum(e => e.Frais.Sum(fr => fr.MontantPaye)),
                f.Eleves.Sum(e => e.Frais.Sum(fr => fr.Montant)) - f.Eleves.Sum(e => e.Frais.Sum(fr => fr.MontantPaye)),
                CalculerStatut(
                    f.Eleves.Sum(e => e.Frais.Sum(fr => fr.Montant)),
                    f.Eleves.Sum(e => e.Frais.Sum(fr => fr.MontantPaye))
                )
            ))
            .ToListAsync(ct);

        return Result<List<FamilleDto>>.Success(familles);
    }

    private string CalculerStatut(decimal totalDu, decimal totalPaye)
    {
        var solde = totalDu - totalPaye;
        if (solde <= 0) return "Payé";
        if (totalPaye > 0) return "Partiel";
        return "Impayé";
    }
}