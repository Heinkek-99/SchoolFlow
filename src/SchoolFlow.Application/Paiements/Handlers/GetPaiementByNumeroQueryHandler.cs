using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Paiements.Queries;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Paiements.Handlers;


public class GetPaiementByNumeroQueryHandler : IRequestHandler<GetPaiementByNumeroQuery, Result<PaiementDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPaiementByNumeroQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaiementDetailDto>> Handle(GetPaiementByNumeroQuery request, CancellationToken ct)
    {
        var paiement = await _context.Paiements
            .Include(p => p.Famille)
            .Include(p => p.Ventilations)
                .ThenInclude(v => v.Frais)
                    .ThenInclude(f => f.Eleve)
            .Include(p => p.Ventilations)
                .ThenInclude(v => v.Frais)
                    .ThenInclude(f => f.TypeFrais)
            .FirstOrDefaultAsync(p => p.NumeroPaiement == request.Numero, ct);

        if (paiement == null)
            return Result<PaiementDetailDto>.Failure("Paiement introuvable");

        var ventilations = paiement.Ventilations.Select(v => new VentilationDetailDto(
            $"{v.Frais.Eleve.Prenom} {v.Frais.Eleve.Nom}",
            v.Frais.TypeFrais.Libelle,
            v.MontantVentile
        )).ToList();

        var dto = new PaiementDetailDto(
            paiement.Id,
            paiement.NumeroPaiement,
            paiement.DatePaiement,
            paiement.MontantTotal,
            paiement.ModePaiement.ToString(),
            $"{paiement.Famille.NomPere} {paiement.Famille.PrenomPere}",
            ventilations
        );

        return Result<PaiementDetailDto>.Success(dto);
    }
}
