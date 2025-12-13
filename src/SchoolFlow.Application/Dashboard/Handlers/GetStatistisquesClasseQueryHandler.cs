using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Dashboard.Queries;
using SchoolFlow.Shared.Dtos;
namespace SchoolFlow.Application.Dashboard.Handlers;

public class GetStatistiquesClasseQueryHandler : IRequestHandler<GetStatistiquesClasseQuery, Result<StatistiquesClasseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStatistiquesClasseQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StatistiquesClasseDto>> Handle(GetStatistiquesClasseQuery request, CancellationToken ct)
    {
        var classe = await _context.Classes
            .Include(c => c.Eleves.Where(e => !e.IsArchived && e.Statut == Domain.Entities.StatutEleve.Actif))
                .ThenInclude(e => e.Frais.Where(f => !f.IsArchived))
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.ClasseId, ct);

        if (classe == null)
            return Result<StatistiquesClasseDto>.Failure("Classe introuvable");

        var eleves = classe.Eleves.ToList();
        var effectif = eleves.Count;
        var garcons = eleves.Count(e => e.Sexe == Domain.Entities.Sexe.Masculin);
        var filles = eleves.Count(e => e.Sexe == Domain.Entities.Sexe.Feminin);
        
        var ages = eleves.Select(e =>
        {
            var age = DateTime.Today.Year - e.DateNaissance.Year;
            if (DateTime.Today < e.DateNaissance.AddYears(age)) age--;
            return age;
        }).ToList();

        var moyenneAge = ages.Any() ? ages.Average() : 0;

        var totalDu = eleves.Sum(e => e.Frais.Sum(f => f.Montant));
        var totalPaye = eleves.Sum(e => e.Frais.Sum(f => f.MontantPaye));
        var tauxRecouvrement = totalDu > 0 ? (totalPaye / totalDu) * 100 : 0;

        var elevesAJour = eleves.Count(e => e.Frais.Sum(f => f.Montant - f.MontantPaye) <= 0);
        var elevesImpayes = effectif - elevesAJour;

        var stats = new StatistiquesClasseDto(
            effectif,
            garcons,
            filles,
            classe.CapaciteMax > 0 ? ((decimal)effectif / classe.CapaciteMax) * 100 : 0,
            Math.Round((decimal)moyenneAge, 1),
            totalDu,
            totalPaye,
            Math.Round(tauxRecouvrement, 2),
            elevesAJour,
            elevesImpayes
        );

        return Result<StatistiquesClasseDto>.Success(stats);
    }
}