using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.TypesFrais.Commands;

namespace SchoolFlow.Application.TypesFrais.Handlers;

public class ArchiveTypeFraisCommandHandler : IRequestHandler<ArchiveTypeFraisCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ArchiveTypeFraisCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(ArchiveTypeFraisCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var typeFrais = await _context.TypeFrais
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.EcoleId == ecoleId, ct);

        if (typeFrais is null)
            return Result<string>.Failure("Type de frais introuvable.");

        if (typeFrais.IsArchived)
            return Result<string>.Failure("Ce type de frais est déjà archivé.");

        // Refuser si des frais avec des paiements existent dans l'année active
        var anneeActiveId = await _context.AnneeScolaires
            .Where(a => a.EcoleId == ecoleId && a.IsActive)
            .Select(a => a.Id)
            .FirstOrDefaultAsync(ct);

        if (anneeActiveId != Guid.Empty)
        {
            var aPaiementsLies = await _context.Frais
                .AnyAsync(f => f.TypeFraisId == request.Id
                            && f.EcoleId == ecoleId
                            && f.MontantPaye > 0
                            && f.Eleve.AnneeScolaireId == anneeActiveId, ct);

            if (aPaiementsLies)
                return Result<string>.Failure(
                    "Impossible d'archiver : des paiements sont liés à ce type de frais dans l'année scolaire active.");
        }

        typeFrais.IsArchived = true;
        typeFrais.ArchivedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return Result<string>.Success($"Type de frais '{typeFrais.Libelle}' archivé.");
    }
}
