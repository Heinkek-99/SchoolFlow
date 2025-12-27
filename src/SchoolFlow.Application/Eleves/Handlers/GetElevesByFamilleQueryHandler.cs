using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Eleves.Queries;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Eleves.Handlers;

public class GetElevesByFamilleQueryHandler : IRequestHandler<GetElevesByFamilleQuery, Result<List<EleveSimpleDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetElevesByFamilleQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<EleveSimpleDto>>> Handle(GetElevesByFamilleQuery request, CancellationToken ct)
    {
        // Vérifier si la famille existe
        var familleExists = await _context.Familles
            .AnyAsync(f => f.Id == request.FamilleId && !f.IsArchived, ct);

        if (!familleExists)
        {
            return Result<List<EleveSimpleDto>>.Failure(
                $"Famille avec l'ID {request.FamilleId} introuvable"
            );
        }        
        
        var eleves = await _context.Eleves
            .Include(e => e.Frais.Where(f => !f.IsArchived))
            .Where(e => e.FamilleId == request.FamilleId && !e.IsArchived)
            .OrderBy(e => e.Nom)
            .ThenBy(e => e.Prenom)
            .AsNoTracking()
            .ToListAsync(ct);

        var result = eleves.Select(e => new EleveSimpleDto(
            e.Id,
            e.Matricule ?? "N/A",
            $"{e.Prenom} {e.Nom}",
            e.DateNaissance,
            e.Sexe.ToString(),
            e.Classe != null ? e.Classe.Nom : "Non assogné",
            e.PhotoPath,
            e.Frais.Sum(f => f.Montant - f.MontantPaye)
        )).ToList();

        return Result<List<EleveSimpleDto>>.Success(result);
    }
}