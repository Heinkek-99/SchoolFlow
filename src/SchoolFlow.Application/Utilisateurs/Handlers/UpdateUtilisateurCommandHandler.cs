using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Utilisateurs.Commands;

namespace SchoolFlow.Application.Utilisateurs.Handlers;

public class UpdateUtilisateurCommandHandler : IRequestHandler<UpdateUtilisateurCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateUtilisateurCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(UpdateUtilisateurCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var user = await _context.Utilisateurs
            .FirstOrDefaultAsync(u => u.Id == request.Id && u.EcoleId == ecoleId, ct);

        if (user is null)
            return Result<string>.Failure("Utilisateur introuvable.");

        user.MettreAJour(request.Nom, request.Prenom, request.Email, request.Telephone);
        await _context.SaveChangesAsync(ct);

        return Result<string>.Success("Utilisateur mis à jour.");
    }
}
