using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.AnneeScolaires.Commands;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;

namespace SchoolFlow.Application.AnneeScolaires.Handlers;

public class UpdateAnneeScolaireCommandHandler : IRequestHandler<UpdateAnneeScolaireCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateAnneeScolaireCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(UpdateAnneeScolaireCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var annee = await _context.AnneeScolaires
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.EcoleId == ecoleId, ct);

        if (annee is null)
            return Result<string>.Failure("Année scolaire introuvable.");

        if (annee.IsActive)
            return Result<string>.Failure("Impossible de modifier une année scolaire active.");

        if (request.DateFin <= request.DateDebut)
            return Result<string>.Failure("La date de fin doit être postérieure à la date de début.");

        var libelleExiste = await _context.AnneeScolaires
            .AnyAsync(a => a.EcoleId == ecoleId
                        && a.Libelle == request.Libelle.Trim()
                        && a.Id != request.Id, ct);

        if (libelleExiste)
            return Result<string>.Failure($"Une année scolaire '{request.Libelle}' existe déjà.");

        annee.MettreAJour(request.Libelle, request.DateDebut, request.DateFin);
        await _context.SaveChangesAsync(ct);

        return Result<string>.Success($"Année scolaire '{annee.Libelle}' mise à jour.");
    }
}
