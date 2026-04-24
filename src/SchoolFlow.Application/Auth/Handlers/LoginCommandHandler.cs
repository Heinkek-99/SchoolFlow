using MediatR;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Auth.Commands;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtService;

    public LoginCommandHandler(IApplicationDbContext context, IJwtTokenService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _context.Utilisateurs
            .Include(u => u.Ecole)
            .FirstOrDefaultAsync(u => u.Username == request.Username && !u.IsArchived, ct);

        if (user is null)
            return Result<LoginResponse>.Failure("Nom d'utilisateur ou mot de passe incorrect.");

        // SuperAdmin n'appartient à aucune école — pas de vérification de statut école
        if (user.Role != SchoolFlow.Domain.Entities.Role.SuperAdmin &&
            (user.Ecole is null || user.Ecole.Statut != SchoolFlow.Domain.Entities.StatutEcole.Active))
            return Result<LoginResponse>.Failure(
                "Votre établissement n'est pas encore activé. Contactez l'administrateur.");

        if (!user.IsActive)
            return Result<LoginResponse>.Failure("Compte désactivé. Contactez l'administrateur.");

        if (user.EstVerrouille)
            return Result<LoginResponse>.Failure(
                $"Compte verrouillé jusqu'à {user.LockedUntil!.Value:HH:mm}. " +
                "Trop de tentatives incorrectes.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            // ← Logique métier dans l'entité, plus dans le handler
            var verrouille = user.EnregistrerEchecConnexion();
            await _context.SaveChangesAsync(ct);

            return verrouille
                ? Result<LoginResponse>.Failure("Compte verrouillé après 5 tentatives incorrectes.")
                : Result<LoginResponse>.Failure("Nom d'utilisateur ou mot de passe incorrect.");
        }

        // Connexion réussie — logique dans l'entité
        user.EnregistrerConnexion();
        await _context.SaveChangesAsync(ct);

        var token = _jwtService.GenerateToken(user);

        var nomEcole = user.Role == SchoolFlow.Domain.Entities.Role.SuperAdmin
            ? "SchoolFlow HQ"
            : user.Ecole?.Nom ?? string.Empty;

        return Result<LoginResponse>.Success(new LoginResponse(
            user.Id,
            user.EcoleId,
            nomEcole,
            user.Username,
            user.Nom,
            user.Prenom,
            user.Role.ToString(),
            token
        ));
    }
}