using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Utilisateurs.Commands;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Utilisateurs.Handlers;

public class ArchiverUtilisateurCommandHandler : IRequestHandler<ArchiverUtilisateurCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ArchiverUtilisateurCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(ArchiverUtilisateurCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var user = await _context.Utilisateurs
            .FirstOrDefaultAsync(u => u.Id == request.Id && u.EcoleId == ecoleId, ct);

        if (user is null)
            return Result<string>.Failure("Utilisateur introuvable.");

        // Empêcher la suppression du dernier Admin
        if (user.Role == Role.Admin)
        {
            var nbAdmins = await _context.Utilisateurs
                .CountAsync(u => u.EcoleId == ecoleId && u.Role == Role.Admin && !u.IsArchived, ct);

            if (nbAdmins <= 1)
                return Result<string>.Failure("Impossible de supprimer le dernier administrateur de l'école.");
        }

        user.IsArchived = true;
        user.ArchivedAt = DateTime.UtcNow;
        user.Desactiver();
        await _context.SaveChangesAsync(ct);

        return Result<string>.Success($"Utilisateur {user.Username} archivé.");
    }
}
