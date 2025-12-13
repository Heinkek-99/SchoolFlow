using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Paiements.Queries;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Paiements.Handlers;

public class ProposerVentilationQueryHandler : IRequestHandler<ProposerVentilationQuery, Result<List<VentilationProposeeDto>>>
{
    private readonly IApplicationDbContext _context;

    public ProposerVentilationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<VentilationProposeeDto>>> Handle(ProposerVentilationQuery request, CancellationToken ct)
    {
        var famille = await _context.Familles
            .Include(f => f.Eleves)
                .ThenInclude(e => e.Frais)
            .FirstOrDefaultAsync(f => f.Id == request.FamilleId && !f.IsArchived, ct);

        if (famille == null)
            return Result<List<VentilationProposeeDto>>.Failure("Famille introuvable");

        var propositions = new List<VentilationProposeeDto>();
        var montantRestant = request.MontantTotal;

        // Calculer les soldes de chaque élève
        foreach (var eleve in famille.Eleves.Where(e => !e.IsArchived).OrderBy(e => e.Nom))
        {
            var soldeEleve = eleve.Frais.Where(f => !f.IsArchived).Sum(f => f.Montant - f.MontantPaye);

            if (soldeEleve <= 0) 
                continue;

            var montantPropose = Math.Min(soldeEleve, montantRestant);

            propositions.Add(new VentilationProposeeDto(
                EleveId: eleve.Id,
                NomCompletEleve: $"{eleve.Prenom} {eleve.Nom}",
                Matricule: eleve.Matricule,
                SoldeRestant: soldeEleve,
                MontantPropose: montantPropose,
                Justification: montantPropose >= soldeEleve
                    ? "Paiement complet du solde"
                    : $"Paiement partiel ({montantPropose / soldeEleve * 100:F1}%)"
            ));

            montantRestant -= montantPropose;

            if (montantRestant <= 0.01m)
                break;
        }

        // Si aucune proposition n'a été créée
        if (!propositions.Any())
        {
            return Result<List<VentilationProposeeDto>>.Failure(
                "Aucun élève de cette famille n'a de frais impayés. " +
                "Tous les soldes sont à jour."
            );
        }

        // Si il reste du montant non ventilé (excédent)
        if (montantRestant > 0.01m)
        {
            // Ajouter une note dans la justification de la dernière proposition
            var derniere = propositions.Last();
            propositions[propositions.Count - 1] = derniere with
            {
                Justification = $"{derniere.Justification} (Excédent de {montantRestant:N0} FCFA non ventilé)"
            };
        }

        return Result<List<VentilationProposeeDto>>.Success(propositions);
    }
}