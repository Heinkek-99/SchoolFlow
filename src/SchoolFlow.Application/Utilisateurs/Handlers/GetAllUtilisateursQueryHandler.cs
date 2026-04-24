using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Utilisateurs.Queries;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Utilisateurs.Handlers;

public class GetAllUtilisateursQueryHandler : IRequestHandler<GetAllUtilisateursQuery, Result<List<UtilisateurDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAllUtilisateursQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<UtilisateurDto>>> Handle(GetAllUtilisateursQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var query = _context.Utilisateurs
            .AsNoTracking()
            .Where(u => u.EcoleId == ecoleId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(u =>
                u.Nom.ToLower().Contains(s) ||
                u.Prenom.ToLower().Contains(s) ||
                u.Username.ToLower().Contains(s));
        }

        var users = await query
            .OrderBy(u => u.Nom)
            .ThenBy(u => u.Prenom)
            .Select(u => new UtilisateurDto(
                u.Id, u.Username, u.Nom, u.Prenom,
                $"{u.Prenom} {u.Nom}",
                u.Email, u.Telephone,
                u.Role.ToString(),
                u.IsActive, u.LastLoginAt, u.CreatedAt))
            .ToListAsync(ct);

        return Result<List<UtilisateurDto>>.Success(users);
    }
}
