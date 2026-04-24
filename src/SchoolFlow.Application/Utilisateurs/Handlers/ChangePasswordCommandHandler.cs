using BCrypt.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Utilisateurs.Commands;

namespace SchoolFlow.Application.Utilisateurs.Handlers;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ChangePasswordCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var user = await _context.Utilisateurs
            .FirstOrDefaultAsync(u => u.Id == request.UtilisateurId && u.EcoleId == ecoleId, ct);

        if (user is null)
            return Result<string>.Failure("Utilisateur introuvable.");

        if (!BCrypt.Net.BCrypt.Verify(request.AncienMotDePasse, user.PasswordHash))
            return Result<string>.Failure("Ancien mot de passe incorrect.");

        var newHash = BCrypt.Net.BCrypt.HashPassword(request.NouveauMotDePasse, workFactor: 12);
        user.MettreAJourMotDePasse(newHash);
        await _context.SaveChangesAsync(ct);

        return Result<string>.Success("Mot de passe modifié avec succès.");
    }
}
