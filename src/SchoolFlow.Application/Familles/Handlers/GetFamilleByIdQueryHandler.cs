using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Familles.Queries;
using SchoolFlow.Shared.Dtos;

public class GetFamilleByIdQueryHandler : IRequestHandler<GetFamilleByIdQuery, Result<FamilleDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFamilleByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<FamilleDetailDto>> Handle(GetFamilleByIdQuery request, CancellationToken ct)
    {
        var famille = await _context.Familles
            .AsSplitQuery()
            .Include(f => f.Eleves.Where(e => !e.IsArchived))
                .ThenInclude(e => e.Classe)
            .Include(f => f.Eleves.Where(e => !e.IsArchived))
                .ThenInclude(e => e.Frais.Where(fr => !fr.IsArchived))
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.Id, ct);

        if (famille == null)
            return Result<FamilleDetailDto>.Failure("Famille introuvable");

        var enfants = famille.Eleves
            .Where(e => !e.IsArchived)
            .Select(e => new EnfantDto(
                e.Id,
                e.Nom,
                e.Prenom,
                e.Matricule,
                e.Classe.Nom,
                e.Frais.Sum(f => f.Montant - f.MontantPaye)
            ))
            .ToList();

        var totalDu = famille.Eleves
            .Where(e => !e.IsArchived)
            .Sum(e => e.Frais.Sum(f => f.Montant));

        var totalPaye = famille.Eleves
            .Where(e => !e.IsArchived)
            .Sum(e => e.Frais.Sum(f => f.MontantPaye));

        var dto = new FamilleDetailDto(
            famille.Id,
            famille.NomPere,
            famille.PrenomPere,
            famille.TelephonePere,
            famille.EmailPere,
            famille.NomMere,
            famille.PrenomMere,
            famille.TelephoneMere,
            famille.Adresse,
            famille.Ville,
            famille.TelephonePrincipal,
            enfants,
            totalDu,
            totalPaye,
            totalDu - totalPaye
        );

        return Result<FamilleDetailDto>.Success(dto);
    }
}