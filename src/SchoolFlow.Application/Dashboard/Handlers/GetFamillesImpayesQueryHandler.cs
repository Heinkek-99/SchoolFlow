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
    private readonly ICurrentUserService _currentUser;

    public GetFamillesImpayesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<FamilleImpayeDto>>> Handle(GetFamillesImpayesQuery request, CancellationToken ct)
    {
    // ÉTAPE 1 : Récupérer les familles avec leurs élèves et frais
        // var famillesImpayes = await _context.Familles
        //     .Include(f => f.Eleves)
        //     .ThenInclude(e => e.Frais)
        //     .Where(f => !f.IsArchived && f.Eleves.Any(e => e.Frais.Sum(fr => fr.Montant - fr.MontantPaye) > 0))
        //     .Select(f => new
        //     {
        //         Famille = f,
        //         MontantDu = f.Eleves.Sum(e => e.Frais.Sum(fr => fr.Montant)),
        //         MontantPaye = f.Eleves.Sum(e => e.Frais.Sum(fr => fr.MontantPaye)),
        //         SoldeRestant = f.Eleves.Sum(e => e.Frais.Sum(fr => fr.Montant - fr.MontantPaye)),
        //         DatePlusAncienImpaye = f.Eleves
        //             .SelectMany(e => e.Frais)
        //             .Where(fr => fr.Montant - fr.MontantPaye > 0)
        //             .Min(fr => fr.DateEcheance)
        //     })
        //     .ToListAsync(ct);


        var ecoleId = _currentUser.EcoleId;

        var famillesQuery = _context.Familles
            .Where(f => f.EcoleId == ecoleId && !f.IsArchived)
            .Include(f => f.Eleves.Where(e => !e.IsArchived))
            .ThenInclude(e => e.Frais.Where(fr => !fr.IsArchived))
            .AsSplitQuery()
            .Where(f => f.Eleves.Any(e => 
                e.Frais.Any(fr => fr.Montant - fr.MontantPaye > 0)));
            
        var famillesAvecDonnees = await famillesQuery.ToListAsync(ct);

            // .Select(f => new
            // {
            //     Famille = f,
            //     MontantDu = f.Eleves.Sum(e => e.Frais.Sum(fr => fr.Montant)),
            //     MontantPaye = f.Eleves.Sum(e => e.Frais.Sum(fr => fr.MontantPaye)),
            //     SoldeRestant = f.Eleves.Sum(e => e.Frais.Sum(fr => fr.Montant - fr.MontantPaye)),
            //     DatePlusAncienImpaye = f.Eleves
            //         .SelectMany(e => e.Frais)
            //         .Where(fr => fr.Montant - fr.MontantPaye > 0)
            //         .Min(fr => (DateTime?)fr.DateEcheance) // Utilisation de DateTime? pour gérer les familles sans impayés
            // })
            // .Where(x => x.SoldeRestant > 0) // Filtrer les familles avec un solde restant positif
            // .ToListAsync(ct);
        
        // ÉTAPE 2 : Calculer les montants dus, payés, soldes restants et autres infos

        var result = famillesAvecDonnees.Select(x =>
        {
            var tousLesFrais = x.Eleves.SelectMany(e => e.Frais).ToList();
            
            var MontantDu = tousLesFrais.Sum(fr => fr.Montant);
            var MontantPaye = tousLesFrais.Sum(fr => fr.MontantPaye);
            var SoldeRestant = MontantDu - MontantPaye;

            var DatePlusAncienImpaye = tousLesFrais
                .Where(fr => fr.Montant - fr.MontantPaye > 0)
                .Min(fr => fr.DateEcheance);

            var prochaineEcheance = tousLesFrais
                .Where(fr => fr.Montant - fr.MontantPaye > 0 && fr.DateEcheance > DateTime.Now)
                .OrderBy(fr => fr.DateEcheance)
                .FirstOrDefault()?.DateEcheance;

            // var joursImpaye = DatePlusAncienImpaye != null
            //     ? (DateTime.Now - DatePlusAncienImpaye).Days
            //     : 0;
                
            // var priorite = joursImpaye > 90 ? "Critique" : joursImpaye > 30 ? "Élevée" : "Normale";
            var joursRetard = prochaineEcheance.HasValue && prochaineEcheance.Value < DateTime.Now
                ? (DateTime.Now - prochaineEcheance.Value).Days
                : 0;

            return new FamilleImpayeDto(
                FamilleId: x.Id,
                NomFamille: x.NomFamille,
                Telephone: x.TelephonePere ?? x.TelephoneMere ?? "Non renseigné",
                NombreEnfants: x.Eleves.Count,
                MontantDu: MontantDu,
                MontantPaye: MontantPaye,
                SoldeRestant: SoldeRestant,
                prochaineEcheance,
                joursRetard
            );
        })
        .Where(f => request.JoursRetardMinimum == null || f.JoursRetard >= request.JoursRetardMinimum)
        .OrderByDescending(f => f.SoldeRestant)
        .ThenByDescending(f => f.JoursRetard)
        .ToList();

        // 3. Limitation des résultats si demandée
        if (request.LimiteResultats.HasValue && request.LimiteResultats.Value > 0)
        {
            result = result.Take(request.LimiteResultats.Value).ToList();
        }

        // ÉTAPE 4 : Retourner le résultat
        return Result<List<FamilleImpayeDto>>.Success(result);
    }
}