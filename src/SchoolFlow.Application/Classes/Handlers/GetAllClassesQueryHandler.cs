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
    private readonly ICurrentUserService _currentUser;

    public GetAllClassesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<ClasseDto>>> Handle(GetAllClassesQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var classes = await _context.Classes
            .Include(c => c.Eleves.Where(e => !e.IsArchived && e.Statut == Domain.Entities.StatutEleve.Actif))
            .Include(c => c.AnneeScolaire)
            .Where(c => c.EcoleId == ecoleId && c.AnneeScolaire.IsActive)
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
                NomComplet: string.IsNullOrWhiteSpace(c.Section) ? c.Nom : $"{c.Nom} {c.Section}",
                c.Niveau.ToString(),
                c.SousSysteme.ToString(),
                c.Section,
                effectif,
                c.CapaciteMax,
                Math.Max(0, c.CapaciteMax - effectif),
                EstComplete: effectif >= c.CapaciteMax,
                c.FraisScolarite,
                c.Statut.ToString(),
                c.AnneeScolaireId,
                c.AnneeScolaire.Libelle
            );
        }).ToList();

        return Result<List<ClasseDto>>.Success(result);
    }
}
