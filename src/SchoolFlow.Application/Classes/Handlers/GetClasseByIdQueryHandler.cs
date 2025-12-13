using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Classes.Queries;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Classes.Handlers;

public class GetClasseByIdQueryHandler : IRequestHandler<GetClasseByIdQuery, Result<ClasseDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetClasseByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ClasseDetailDto>> Handle(GetClasseByIdQuery request, CancellationToken ct)
    {
        var classe = await _context.Classes
            .AsSplitQuery()
            .Include(c => c.AnneeScolaire)
            .Include(c => c.Eleves.Where(e => !e.IsArchived && e.Statut == Domain.Entities.StatutEleve.Actif))
                .ThenInclude(e => e.Frais.Where(f => !f.IsArchived))
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, ct);

        if (classe == null)
            return Result<ClasseDetailDto>.Failure("Classe introuvable");

        var elevesDto = classe.Eleves
            .OrderBy(e => e.Nom)
            .ThenBy(e => e.Prenom)
            .Select(e =>
            {
                var solde = e.Frais.Sum(f => f.Montant - f.MontantPaye);
                var age = DateTime.Today.Year - e.DateNaissance.Year;
                if (DateTime.Today < e.DateNaissance.AddYears(age)) age--;

                return new EleveClasseDto(
                    e.Id,
                    e.Matricule,
                    $"{e.Prenom} {e.Nom}",
                    e.Sexe.ToString(),
                    age,
                    solde,
                    solde <= 0 ? "À jour" : solde < 50000 ? "Partiel" : "Impayé"
                );
            })
            .ToList();

        var dto = new ClasseDetailDto(
            classe.Id,
            classe.Code,
            classe.Nom,
            classe.Niveau.ToString(),
            classe.Section,
            classe.Eleves.Count,
            classe.CapaciteMax,
            elevesDto,
            classe.AnneeScolaire.Libelle
        );

        return Result<ClasseDetailDto>.Success(dto);
    }
}