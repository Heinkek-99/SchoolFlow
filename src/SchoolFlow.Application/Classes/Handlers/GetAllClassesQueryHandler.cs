using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models; 
using SchoolFlow.Application.Classes.Queries;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Classes.Handlers;

public class GetAllClassesQueryHandler : IRequestHandler<GetAllClassesQuery, Result<List<ClasseDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllClassesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ClasseDto>>> Handle(GetAllClassesQuery request, CancellationToken ct)
    {
        var classes = await _context.Classes
            .Include(c => c.Eleves.Where(e => !e.IsArchived && e.Statut == Domain.Entities.StatutEleve.Actif))
            .Where(c => c.AnneeScolaire.IsActive)
            .OrderBy(c => c.Niveau)
            .ThenBy(c => c.Section)
            .AsNoTracking()
            .ToListAsync(ct);

        var result = classes.Select(c =>
        {
            var effectif = c.Eleves.Count(e => !e.IsArchived);
            return new ClasseDto(
                c.Id,
                c.Code,
                c.Nom,
                c.Niveau.ToString(),
                effectif,
                c.CapaciteMax,
                effectif >= c.CapaciteMax
            );
        }).ToList();

        return Result<List<ClasseDto>>.Success(result);
    }
}