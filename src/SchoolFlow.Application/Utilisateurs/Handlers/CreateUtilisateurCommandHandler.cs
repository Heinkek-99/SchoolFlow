using BCrypt.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Utilisateurs.Commands;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Utilisateurs.Handlers;

public class CreateUtilisateurCommandHandler : IRequestHandler<CreateUtilisateurCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateUtilisateurCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateUtilisateurCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        // Seul le SuperAdmin peut créer des utilisateurs pour d'autres écoles
        if (ecoleId == Guid.Empty && !_currentUser.IsSuperAdmin)
            return Result<Guid>.Failure("EcoleId non trouvé dans le contexte.");

        // Rôle SuperAdmin non assignable via cette route
        if (request.Role == Role.SuperAdmin)
            return Result<Guid>.Failure("Le rôle SuperAdmin ne peut pas être assigné via cette route.");

        var usernameExiste = await _context.Utilisateurs
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Username == request.Username.ToLower(), ct);

        if (usernameExiste)
            return Result<Guid>.Failure($"Le nom d'utilisateur '{request.Username}' est déjà utilisé.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        var utilisateur = Utilisateur.Creer(
            ecoleId: ecoleId,
            username: request.Username,
            passwordHash: passwordHash,
            nom: request.Nom,
            prenom: request.Prenom,
            role: request.Role,
            email: request.Email,
            telephone: request.Telephone);

        _context.Utilisateurs.Add(utilisateur);
        await _context.SaveChangesAsync(ct);

        return Result<Guid>.Success(utilisateur.Id);
    }
}
