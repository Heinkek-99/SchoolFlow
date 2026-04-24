using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Utilisateurs.Queries;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Utilisateurs.Handlers;

public class GetUtilisateurByIdQueryHandler : IRequestHandler<GetUtilisateurByIdQuery, Result<UtilisateurDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUtilisateurByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<UtilisateurDto>> Handle(GetUtilisateurByIdQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var user = await _context.Utilisateurs
            .AsNoTracking()
            .Where(u => u.Id == request.Id && u.EcoleId == ecoleId)
            .Select(u => new UtilisateurDto(
                u.Id, u.Username, u.Nom, u.Prenom,
                $"{u.Prenom} {u.Nom}",
                u.Email, u.Telephone,
                u.Role.ToString(),
                u.IsActive, u.LastLoginAt, u.CreatedAt))
            .FirstOrDefaultAsync(ct);

        if (user is null)
            return Result<UtilisateurDto>.Failure("Utilisateur introuvable.");

        return Result<UtilisateurDto>.Success(user);
    }
}
