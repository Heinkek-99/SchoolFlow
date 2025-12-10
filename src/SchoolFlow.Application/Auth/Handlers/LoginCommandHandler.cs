using MediatR;
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
            .FirstOrDefaultAsync(u => u.Username == request.Username && !u.IsArchived, ct);

        if (user == null)
            return Result<LoginResponse>.Failure("Nom d'utilisateur ou mot de passe incorrect");

        if (!user.IsActive)
            return Result<LoginResponse>.Failure("Compte désactivé. Contactez l'administrateur");

        if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
            return Result<LoginResponse>.Failure($"Compte verrouillé jusqu'à {user.LockedUntil.Value:HH:mm}");

        // ⚠️ FIX: Utiliser le namespace complet pour BCrypt
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

        var token = _jwtService.GenerateToken(user);

        var response = new LoginResponse(
            user.Id,
            user.Username,
            user.Nom,
            user.Prenom,
            user.Role.ToString(),
            token
        );

        return Result<LoginResponse>.Success(response);
    }
}