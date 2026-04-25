using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Eleves.Queries;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Eleves.Handlers;

public class GetEleveDossierQueryHandler
    : IRequestHandler<GetElevesDossierQuery, Result<EleveDossierDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEleveDossierQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<EleveDossierDto>> Handle(
        GetElevesDossierQuery request, CancellationToken ct)
    {
        var eleve = await _context.Eleves
            .Include(e => e.Classe)
            .Include(e => e.Famille)
            .Include(e => e.Frais.Where(f => !f.IsArchived))
                .ThenInclude(f => f.TypeFrais)
            .Include(e => e.Frais.Where(f => !f.IsArchived))
                .ThenInclude(f => f.Periode)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.Id, ct);

        if (eleve is null)
            return Result<EleveDossierDto>.Failure("Élève introuvable.");

        // Derniers paiements de la famille
        var paiements = await _context.Paiements
            .Where(p => p.FamilleId == eleve.FamilleId && !p.IsArchived)
            .Include(p => p.Ventilations)
            .OrderByDescending(p => p.DatePaiement)
            .Take(5)
            .AsNoTracking()
            .ToListAsync(ct);

        var fraisList = eleve.Frais
            .OrderBy(f => f.DateEcheance)
            .Select(f => new FraisDto(
                f.Id,
                f.TypeFrais.Libelle,
                f.Montant,
                f.MontantPaye,
                f.DateEcheance,
                DateTime.UtcNow > f.DateEcheance && f.Montant > f.MontantPaye,
                f.Periode?.Libelle))
            .ToList();

        var derniersPaiements = paiements
            .Select(p => new PaiementDetailDto(
                p.Id,
                p.NumeroPaiement,
                p.DatePaiement,
                p.MontantTotal,
                p.ModePaiement.ToString(),
                eleve.Famille.NomFamille,
                p.Ventilations.Select(v => new VentilationDetailDto(
                    eleve.NomComplet,
                    "",
                    v.MontantVentile))
                .ToList()))
            .ToList();

        var totalDu = eleve.Frais.Sum(f => f.Montant);
        var totalPaye = eleve.Frais.Sum(f => f.MontantPaye);
        var solde = totalDu - totalPaye;
        var tauxPaiement = totalDu > 0 ? Math.Round(totalPaye / totalDu * 100, 2) : 0m;

        var dto = new EleveDossierDto(
            eleve.Id,
            eleve.Matricule,
            eleve.Nom,
            eleve.Prenom,
            eleve.NomComplet,
            eleve.DateNaissance,
            eleve.LieuNaissance,
            eleve.Sexe.ToString(),
            eleve.PhotoPath,
            eleve.Nationalite,
            eleve.GroupeSanguin,
            eleve.ContactUrgence,
            eleve.Statut.ToString(),
            eleve.Classe?.Nom,
            eleve.Classe?.SousSysteme.ToString(),
            eleve.Classe?.Section,
            eleve.FamilleId,
            eleve.Famille.NomFamille,
            eleve.Famille.TelephonePrincipal,
            totalDu,
            totalPaye,
            solde,
            tauxPaiement,
            fraisList,
            derniersPaiements
        );

        return Result<EleveDossierDto>.Success(dto);
    }
}
