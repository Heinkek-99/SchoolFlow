using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.AnneeScolaires.Commands;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;

namespace SchoolFlow.Application.AnneeScolaires.Handlers;

public class ActiverAnneeScolaireCommandHandler : IRequestHandler<ActiverAnneeScolaireCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ActiverAnneeScolaireCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(ActiverAnneeScolaireCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var annee = await _context.AnneeScolaires
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.EcoleId == ecoleId, ct);

        if (annee is null)
            return Result<string>.Failure("Année scolaire introuvable.");

        // Règle critique : une seule année active par école
        var autresActives = await _context.AnneeScolaires
            .Where(a => a.EcoleId == ecoleId && a.IsActive && a.Id != request.Id)
            .ToListAsync(ct);

        foreach (var a in autresActives)
            a.Desactiver();

        annee.Activer();
        await _context.SaveChangesAsync(ct);

        return Result<string>.Success($"Année '{annee.Libelle}' activée. {autresActives.Count} année(s) désactivée(s).");
    }
}
