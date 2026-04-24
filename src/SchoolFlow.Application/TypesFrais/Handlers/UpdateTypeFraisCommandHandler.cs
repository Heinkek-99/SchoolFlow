using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.TypesFrais.Commands;

namespace SchoolFlow.Application.TypesFrais.Handlers;

public class UpdateTypeFraisCommandHandler : IRequestHandler<UpdateTypeFraisCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateTypeFraisCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(UpdateTypeFraisCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var typeFrais = await _context.TypeFrais
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.EcoleId == ecoleId, ct);

        if (typeFrais is null)
            return Result<string>.Failure("Type de frais introuvable.");

        typeFrais.Libelle = request.Libelle.Trim();
        typeFrais.Description = request.Description?.Trim();
        typeFrais.IsObligatoire = request.IsObligatoire;
        typeFrais.GenerationAutomatique = request.GenerationAutomatique;
        typeFrais.MontantsParNiveau = request.MontantsParNiveau;
        typeFrais.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Result<string>.Success("Type de frais mis à jour.");
    }
}
