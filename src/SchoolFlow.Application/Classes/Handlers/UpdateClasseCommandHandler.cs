using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Classes.Commands;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;

namespace SchoolFlow.Application.Classes.Handlers;

public class UpdateClasseCommandHandler : IRequestHandler<UpdateClasseCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateClasseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(UpdateClasseCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var classe = await _context.Classes
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.EcoleId == ecoleId, ct);

        if (classe is null)
            return Result<string>.Failure("Classe introuvable.");

        if (classe.Statut == Domain.Entities.StatutClasse.Archivee)
            return Result<string>.Failure("Impossible de modifier une classe archivée.");

        classe.MettreAJour(
            request.Nom, request.Niveau,
            request.SousSysteme, request.Section,
            request.CapaciteMax, request.TitulaireId);

        await _context.SaveChangesAsync(ct);

        return Result<string>.Success($"Classe '{classe.Code}' mise à jour.");
    }
}