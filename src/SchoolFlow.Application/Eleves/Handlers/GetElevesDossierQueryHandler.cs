using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Eleves.Queries;
using SchoolFlow.Shared.Dtos;

public class GetEleveDossierQueryHandler : IRequestHandler<GetElevesDossierQuery, Result<EleveDossierDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEleveDossierQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<EleveDossierDto>> Handle(GetElevesDossierQuery request, CancellationToken ct)
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

        if (eleve == null)
            return Result<EleveDossierDto>.Failure("Élève introuvable");

        var fraisList = eleve.Frais
            .OrderBy(f => f.DateEcheance)
            .Select(f => new FraisDto(
                f.Id,
                f.TypeFrais.Libelle,
                f.Montant,
                f.MontantPaye,
                f.DateEcheance,
                DateTime.UtcNow > f.DateEcheance && f.Montant > f.MontantPaye,
                f.Periode?.Libelle
        )).ToList();

        var totalDu = eleve.Frais.Sum(f => f.Montant);
        var totalPaye =  eleve.Frais.Sum(f => f.MontantPaye);
        var totalRestant = totalDu - totalPaye;
        var dto = new EleveDossierDto(
            eleve.Id,
            eleve.Matricule,
            eleve.Nom,
            eleve.Prenom,
            eleve.DateNaissance,
            eleve.LieuNaissance,
            eleve.Sexe.ToString(),
            eleve.PhotoPath,
            eleve.Classe?.Nom,
            $"{eleve.Famille.NomPere} {eleve.Famille.PrenomPere}",
            fraisList,
            totalDu,
            totalPaye,
            totalRestant
        );

        return Result<EleveDossierDto>.Success(dto);
    }
}