using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Eleves.Queries;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Eleves.Handlers;

public record GetElevesByFamilleQuery(Guid FamilleId) : IRequest<Result<List<EleveSimpleDto>>>;

public class GetElevesByFamilleQueryHandler : IRequestHandler<GetElevesByFamilleQuery, Result<List<EleveSimpleDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetElevesByFamilleQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<EleveSimpleDto>>> Handle(GetElevesByFamilleQuery request, CancellationToken ct)
    {
        var eleves = await _context.Eleves
            .Include(e => e.Frais.Where(f => !f.IsArchived))
            .Where(e => e.FamilleId == request.FamilleId && !e.IsArchived)
            .OrderBy(e => e.DateNaissance)
            .AsNoTracking()
            .ToListAsync(ct);

        var result = eleves.Select(e => new EleveSimpleDto(
            e.Id,
            e.Matricule,
            $"{e.Prenom} {e.Nom}",
            e.Sexe.ToString(),
            e.Frais.Sum(f => f.Montant - f.MontantPaye)
        )).ToList();

        return Result<List<EleveSimpleDto>>.Success(result);
    }
}