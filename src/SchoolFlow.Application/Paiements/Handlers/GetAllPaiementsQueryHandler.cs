using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Paiements.Queries;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Paiements.Handlers;

public class GetAllPaiementsQueryHandler : IRequestHandler<GetAllPaiementsQuery, Result<PagedList<PaiementListItemDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllPaiementsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedList<PaiementListItemDto>>> Handle(GetAllPaiementsQuery request, CancellationToken ct)
    {
        var query = _context.Paiements
            .Include(p => p.Famille)
            .Include(p => p.EnregistreParUtilisateur)
            .Include(p => p.Ventilations)
            .Where(p => !p.IsArchived)
            .AsQueryable();

        // Filtres
        if (request.DateDebut.HasValue)
            query = query.Where(p => p.DatePaiement >= request.DateDebut.Value);

        if (request.DateFin.HasValue)
            query = query.Where(p => p.DatePaiement <= request.DateFin.Value);

        if (!string.IsNullOrWhiteSpace(request.ModePaiement))
        {
            if (Enum.TryParse<Domain.Entities.ModePaiement>(request.ModePaiement, out var mode))
            {
                query = query.Where(p => p.ModePaiement == mode);
            }
        }

        if (request.FamilleId.HasValue)
            query = query.Where(p => p.FamilleId == request.FamilleId.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.DatePaiement)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
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

        var pagedList = new PagedList<PaiementListItemDto>(
            Items: items,
            TotalCount: totalCount,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize
        );

        return Result<PagedList<PaiementListItemDto>>.Success(pagedList);
    }
}