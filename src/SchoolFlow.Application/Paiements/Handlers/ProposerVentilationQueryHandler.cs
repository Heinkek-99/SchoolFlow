using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Paiements.Queries;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Paiements.Handlers;

public class ProposerVentilationQueryHandler
    : IRequestHandler<ProposerVentilationQuery, Result<List<VentilationProposeeDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ProposerVentilationQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<VentilationProposeeDto>>> Handle(
        ProposerVentilationQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        if (ecoleId == Guid.Empty)
            return Result<List<VentilationProposeeDto>>.Failure("Contexte école manquant.");

        var famille = await _context.Familles
            .Include(f => f.Eleves.Where(e => !e.IsArchived))
                .ThenInclude(e => e.Frais.Where(fr => !fr.IsArchived))
                    .ThenInclude(fr => fr.TypeFrais)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.FamilleId && f.EcoleId == ecoleId, ct);

        if (famille is null)
            return Result<List<VentilationProposeeDto>>.Failure("Famille introuvable.");

        // FIFO : frais impayés triés par DateEcheance ASC (les plus anciens d'abord)
        var fraisImpayes = famille.Eleves
            .SelectMany(e => e.Frais
                .Where(f => f.Solde > 0.01m)
                .Select(f => new { Eleve = e, Frais = f }))
            .OrderBy(x => x.Frais.DateEcheance)
            .ThenBy(x => x.Eleve.Nom)
            .ToList();

        if (!fraisImpayes.Any())
            return Result<List<VentilationProposeeDto>>.Failure(
                "Aucun élève de cette famille n'a de frais impayés.");

        var suggestions = new List<VentilationProposeeDto>();
        var montantRestant = request.MontantTotal;

        foreach (var item in fraisImpayes)
        {
            if (montantRestant <= 0.01m) break;

            var aImputer = Math.Min(montantRestant, item.Frais.Solde);
            suggestions.Add(new VentilationProposeeDto(
                EleveId: item.Eleve.Id,
                EleveNom: item.Eleve.NomComplet,
                FraisId: item.Frais.Id,
                LibelleFrais: item.Frais.TypeFrais?.Libelle ?? "",
                MontantSuggere: aImputer,
                SoldeAvant: item.Frais.Solde,
                SoldeApres: item.Frais.Solde - aImputer
            ));
            montantRestant -= aImputer;
        }

        return Result<List<VentilationProposeeDto>>.Success(suggestions);
    }
}
