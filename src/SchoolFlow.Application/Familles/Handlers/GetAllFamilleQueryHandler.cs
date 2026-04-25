using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Familles.Queries;
using SchoolFlow.Shared.Dtos;

public class GetAllFamillesQueryHandler : IRequestHandler<GetAllFamillesQuery, Result<List<FamilleDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GetAllFamillesQueryHandler> _logger;

    public GetAllFamillesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ILogger<GetAllFamillesQueryHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<List<FamilleDto>>> Handle(GetAllFamillesQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        _logger.LogInformation("GetAllFamilles — EcoleId={EcoleId} IsSuperAdmin={IsSuperAdmin}",
            ecoleId, _currentUser.IsSuperAdmin);

        if (ecoleId == Guid.Empty)
            return Result<List<FamilleDto>>.Failure("Accès non autorisé : EcoleId manquant.");

        var familles = await _context.Familles
            .Where(f => f.EcoleId == ecoleId)
            .Include(f => f.Eleves.Where(e => !e.IsArchived))
                .ThenInclude(e => e.Frais.Where(fr => !fr.IsArchived))
            .OrderBy(f => f.NomPere)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
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
