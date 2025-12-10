using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;

namespace SchoolFlow.Application.Auth.Commands;

public record LoginCommand(string Username, string Password) : IRequest<Result<LoginResponse>>;

public record LoginResponse(
    Guid UserId,
    string Username,
    string Nom,
    string Prenom,
    string Role,
    string Token
);

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IApplicationDbContext _context;

    public LoginCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _context.Utilisateurs
            .FirstOrDefaultAsync(u => u.Username == request.Username && !u.IsArchived, ct);

        if (user == null)
            return Result<LoginResponse>.Failure("Nom d'utilisateur ou mot de passe incorrect");

        if (!user.IsActive)
            return Result<LoginResponse>.Failure("Compte désactivé. Contactez l'administrateur");

        if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
            return Result<LoginResponse>.Failure($"Compte verrouillé jusqu'à {user.LockedUntil.Value:HH:mm}");

        // Vérifier mot de passe (BCrypt)
        // if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        // {
        //     user.FailedLoginAttempts++;
        //     if (user.FailedLoginAttempts >= 5)
        //     {
        //         user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
        //     }
        //     await _context.SaveChangesAsync(ct);
        //     return Result<LoginResponse>.Failure("Nom d'utilisateur ou mot de passe incorrect");
        // }

        // Succès - Reset tentatives
        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        var response = new LoginResponse(
            user.Id,
            user.Username,
            user.Nom,
            user.Prenom,
            user.Role.ToString(),
            GenerateJwtToken(user) // À implémenter
        );

        return Result<LoginResponse>.Success(response);
    }

    private string GenerateJwtToken(Domain.Entities.Utilisateur user)
    {
        // Simplification - En prod utiliser JwtSecurityTokenHandler
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}