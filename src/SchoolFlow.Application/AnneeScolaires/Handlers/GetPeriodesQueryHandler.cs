using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.AnneeScolaires.Queries;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.AnneeScolaires.Handlers;

public class GetPeriodesQueryHandler : IRequestHandler<GetPeriodesQuery, Result<List<PeriodeDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetPeriodesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<PeriodeDto>>> Handle(GetPeriodesQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var anneeExiste = await _context.AnneeScolaires
            .AnyAsync(a => a.Id == request.AnneeScolaireId && a.EcoleId == ecoleId, ct);

        if (!anneeExiste)
            return Result<List<PeriodeDto>>.Failure("Année scolaire introuvable.");

        var periodes = await _context.Periodes
            .AsNoTracking()
            .Where(p => p.AnneeScolaireId == request.AnneeScolaireId && p.EcoleId == ecoleId)
            .OrderBy(p => p.Numero)
            .Select(p => new PeriodeDto(
                p.Id, p.Libelle, p.Type.ToString(),
                p.Numero, p.DateDebut, p.DateFin, p.AnneeScolaireId))
            .ToListAsync(ct);

        return Result<List<PeriodeDto>>.Success(periodes);
    }
}
