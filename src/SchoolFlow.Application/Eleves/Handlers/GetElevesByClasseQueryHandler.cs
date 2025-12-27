namespace SchoolFlow.Application.Eleves.Handlers;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Eleves.Queries;
using SchoolFlow.Domain.Entities;
using SchoolFlow.Shared.Dtos;

public class GetEleveByClasseQueryHandler : IRequestHandler<GetElevesByClasseQuery, Result<List<EleveSimpleDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetEleveByClasseQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<EleveSimpleDto>>> Handle(GetElevesByClasseQuery request, CancellationToken  ct)
    {

        var classeExists = await _context.Classes
            .AnyAsync(c => c.Id == request.ClasseId && !c.IsArchived, ct);

        if (!classeExists)
        {
            return Result<List<EleveSimpleDto>>.Failure(
                $"Classe avec l'ID {request.ClasseId} introuvable"
            );
        }

        var eleves = await _context.Eleves
            .Include(e => e.Frais.Where(f => !f.IsArchived))
            .Where(e => e.ClasseId == request.ClasseId && e.Statut == StatutEleve.Actif)
            .OrderBy(e => e.Nom)
            .ThenBy(e => e.Prenom)
            .AsNoTracking()
            .ToListAsync(ct);

        var result = eleves.Select(e => new EleveSimpleDto(
            e.Id,
            e.Matricule,
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
