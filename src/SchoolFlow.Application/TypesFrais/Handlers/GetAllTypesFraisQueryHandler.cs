using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.TypesFrais.Queries;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.TypesFrais.Handlers;

public class GetAllTypesFraisQueryHandler : IRequestHandler<GetAllTypesFraisQuery, Result<List<TypeFraisDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAllTypesFraisQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<TypeFraisDto>>> Handle(GetAllTypesFraisQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var types = await _context.TypeFrais
            .AsNoTracking()
            .Where(t => t.EcoleId == ecoleId)
            .OrderBy(t => t.Categorie)
            .ThenBy(t => t.Code)
            .ToListAsync(ct);

        var result = types.Select(t => new TypeFraisDto(
            t.Id,
            t.Code,
            t.Libelle,
            t.Description,
            t.Categorie.ToString(),
            t.IsRecurrent,
            t.IsObligatoire,
            t.GenerationAutomatique,
            t.MontantsParNiveau.ToDictionary(k => k.Key.ToString(), v => v.Value)
        )).ToList();

        return Result<List<TypeFraisDto>>.Success(result);
    }
}
