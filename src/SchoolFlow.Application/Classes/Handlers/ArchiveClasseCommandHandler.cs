using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Classes.Commands;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;

namespace SchoolFlow.Application.Classes.Handlers;

public class ArchiveClasseCommandHandler : IRequestHandler<ArchiveClasseCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ArchiveClasseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(ArchiveClasseCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var classe = await _context.Classes
            .Include(c => c.Eleves)
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.EcoleId == ecoleId, ct);

        if (classe is null)
            return Result<string>.Failure("Classe introuvable.");

        if (classe.Statut == Domain.Entities.StatutClasse.Archivee)
            return Result<string>.Failure("Cette classe est déjà archivée.");

        var elevesActifs = classe.Eleves.Count(e => !e.IsArchived);
        if (elevesActifs > 0)
            return Result<string>.Failure($"Impossible d'archiver : {elevesActifs} élève(s) encore inscrit(s) dans cette classe.");

        classe.Archiver();
        await _context.SaveChangesAsync(ct);

        return Result<string>.Success($"Classe '{classe.Code}' archivée.");
    }
}