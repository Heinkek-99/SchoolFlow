using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Familles.Queries;
using SchoolFlow.Shared.Dtos;

public class SearchFamillesQueryHandler : IRequestHandler<SearchFamillesQuery, Result<List<FamilleDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SearchFamillesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<FamilleDto>>> Handle(SearchFamillesQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        if (ecoleId == Guid.Empty)
            return Result<List<FamilleDto>>.Failure("Accès non autorisé : EcoleId manquant.");

        var q = request.Query.ToLower();

        var familles = await _context.Familles
            .Where(f => f.EcoleId == ecoleId)
            .Where(f =>
                f.NomPere.ToLower().Contains(q) ||
                (f.PrenomPere != null && f.PrenomPere!.ToLower().Contains(q)) ||
                f.TelephonePrincipal.Contains(q) ||
                (f.TelephonePere != null && f.TelephonePere!.Contains(q)))
            .Include(f => f.Eleves.Where(e => !e.IsArchived))
                .ThenInclude(e => e.Frais.Where(fr => !fr.IsArchived))
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
