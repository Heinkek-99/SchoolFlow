using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.AnneeScolaires.Queries;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.AnneeScolaires.Handlers;

public class GetAllAnneesQueryHandler : IRequestHandler<GetAllAnneesQuery, Result<List<AnneeScolaireDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAllAnneesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<AnneeScolaireDto>>> Handle(GetAllAnneesQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var annees = await _context.AnneeScolaires
            .AsNoTracking()
            .Include(a => a.Periodes)
            .Include(a => a.Classes)
            .Include(a => a.Eleves)
            .Where(a => a.EcoleId == ecoleId)
            .OrderByDescending(a => a.DateDebut)
            .Select(a => new AnneeScolaireDto(
                a.Id,
                a.Libelle,
                a.DateDebut,
                a.DateFin,
                a.IsActive,
                a.Periodes.Count,
                a.Classes.Count(c => !c.IsArchived),
                a.Eleves.Count(e => !e.IsArchived)))
            .ToListAsync(ct);

        return Result<List<AnneeScolaireDto>>.Success(annees);
    }
}
