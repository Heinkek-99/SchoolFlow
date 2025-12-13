using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Paiements.Queries;

namespace SchoolFlow.Application.Paiements.Handlers;
public class GetHistoriquePaiementsFamilleQueryHandler : IRequestHandler<GetHistoriquePaiementsFamilleQuery, Result<List<PaiementListItemDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetHistoriquePaiementsFamilleQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PaiementListItemDto>>> Handle(GetHistoriquePaiementsFamilleQuery request, CancellationToken ct)
    {
        var paiements = await _context.Paiements
            .Include(p => p.Famille)
            .Include(p => p.EnregistreParUtilisateur)
            .Include(p => p.Ventilations)
            .Where(p => !p.IsArchived && p.FamilleId == request.FamilleId)
            .OrderByDescending(p => p.DatePaiement)
            .Select(p => new PaiementListItemDto(
                p.Id,
                p.NumeroPaiement,
                p.Famille.NomFamille,
                p.MontantTotal,
                p.DatePaiement,
                p.ModePaiement.ToString(),
                p.Ventilations.Select(v => v.Frais.EleveId).Distinct().Count(),
                $"{p.EnregistreParUtilisateur.Prenom} {p.EnregistreParUtilisateur.Nom}"
            ))
            .ToListAsync(ct);

        return Result<List<PaiementListItemDto>>.Success(paiements);
    }
} 