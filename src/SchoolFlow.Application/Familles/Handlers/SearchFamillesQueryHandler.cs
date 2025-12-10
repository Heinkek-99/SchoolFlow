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
                .ThenInclude(e => e.Frais)
            .Where(f => 
                f.NomPere.ToLower().Contains(query) ||
                f.PrenomPere!.ToLower().Contains(query) ||
                f.TelephonePrincipal.Contains(query) ||
                f.TelephonePere!.Contains(query))
            .Take(20)
            .Select(f => new FamilleDto(
                f.Id,
                f.NomPere,
                f.PrenomPere,
                f.TelephonePrincipal,
                f.Ville,
                f.Eleves.Count,
                f.Eleves.Sum(e => e.Frais.Sum(fr => fr.Montant)),
                f.Eleves.Sum(e => e.Frais.Sum(fr => fr.MontantPaye)),
                f.Eleves.Sum(e => e.Frais.Sum(fr => fr.Montant - fr.MontantPaye)),
                "Partiel"
            ))
            .ToListAsync(ct);

        return Result<List<FamilleDto>>.Success(familles);
    }
}