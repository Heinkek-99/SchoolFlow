using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Eleves.Queries;
using SchoolFlow.Domain.Entities;
using SchoolFlow.Shared.Dtos;

public class GetAllElevesQueryHandler : IRequestHandler<GetAllElevesQuery, Result<List<EleveDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAllElevesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<EleveDto>>> Handle(GetAllElevesQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        if (ecoleId == Guid.Empty)
            return Result<List<EleveDto>>.Failure("Accès non autorisé : EcoleId manquant.");

        var query = _context.Eleves
            .Where(e => e.EcoleId == ecoleId && !e.IsArchived && e.Statut == StatutEleve.Actif)
            .Include(e => e.Classe)
            .Include(e => e.Famille)
            .Include(e => e.Frais.Where(f => !f.IsArchived))
            .AsQueryable();

        if (request.ClasseId.HasValue)
            query = query.Where(e => e.ClasseId == request.ClasseId.Value);

        var eleves = await query
            .OrderBy(e => e.Nom)
            .ThenBy(e => e.Prenom)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        var result = eleves
            .Select(e => new EleveDto(
                e.Id,
                e.Matricule,
                e.Nom,
                e.Prenom,
                e.Classe?.Nom,
                $"{e.Famille.NomPere} {e.Famille.PrenomPere}",
                e.Frais.Sum(f => f.Montant - f.MontantPaye),
                e.Statut.ToString()
            ))
            .ToList();

        return Result<List<EleveDto>>.Success(result);
    }
}
