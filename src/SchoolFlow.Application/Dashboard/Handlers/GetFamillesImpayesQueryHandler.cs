using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Dashboard.Queries;

namespace SchoolFlow.Application.Dashboard.Handlers;

public class GetFamillesImpayesQueryHandler : IRequestHandler<GetFamillesImpayesQuery, Result<List<FamilleImpayeDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetFamillesImpayesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<FamilleImpayeDto>>> Handle(GetFamillesImpayesQuery request, CancellationToken ct)
    {
        var famillesImpayes = await _context.Familles
            .Include(f => f.Eleves)
                .ThenInclude(e => e.Frais)
            .Where(f => !f.IsArchived && f.Eleves.Any(e => e.Frais.Sum(fr => fr.Montant - fr.MontantPaye) > 0))
            .Select(f => new
            {
                Famille = f,
                MontantDu = f.Eleves.Sum(e => e.Frais.Sum(fr => fr.Montant)),
                MontantPaye = f.Eleves.Sum(e => e.Frais.Sum(fr => fr.MontantPaye)),
                SoldeRestant = f.Eleves.Sum(e => e.Frais.Sum(fr => fr.Montant - fr.MontantPaye)),
                DatePlusAncienImpaye = f.Eleves
                    .SelectMany(e => e.Frais)
                    .Where(fr => fr.Montant - fr.MontantPaye > 0)
                    .Min(fr => fr.DateEcheance)
            })
            .ToListAsync(ct);

        var result = famillesImpayes.Select(x =>
        {
            var joursImpaye = (DateTime.Now - x.DatePlusAncienImpaye).Days;
            var priorite = joursImpaye > 90 ? "Critique" : joursImpaye > 30 ? "Élevée" : "Normale";

            return new FamilleImpayeDto(
                FamilleId: x.Famille.Id,
                NomFamille: x.Famille.NomFamille,
                Telephone: x.Famille.TelephonePere ?? x.Famille.TelephoneMere ?? "Non renseigné",
                NombreEnfants: x.Famille.Eleves.Count,
                MontantDu: x.MontantDu,
                MontantPaye: x.MontantPaye,
                SoldeRestant: x.SoldeRestant,
                DerniereRelance: x.DatePlusAncienImpaye,
                JoursImpaye: joursImpaye,
                Priorite: priorite
            );
        })
        .OrderByDescending(f => f.JoursImpaye)
        .ToList();

        return Result<List<FamilleImpayeDto>>.Success(result);
    }
}