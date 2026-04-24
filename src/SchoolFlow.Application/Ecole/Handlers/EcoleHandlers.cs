using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Ecoles.Commands;
using SchoolFlow.Application.Ecoles.Queries;
using SchoolFlow.Domain.Entities;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Ecoles.Handlers;

// ─── VALIDER ÉCOLE ───────────────────────────────────────────────────────────

public class ValiderEcoleCommandHandler
    : IRequestHandler<ValiderEcoleCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public ValiderEcoleCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<string>> Handle(
        ValiderEcoleCommand request, CancellationToken ct)
    {
        var ecole = await _context.Ecoles
            .FirstOrDefaultAsync(e => e.Id == request.EcoleId, ct);

        if (ecole is null)
            return Result<string>.Failure("École introuvable.");

        try
        {
            ecole.Valider(); // Lève EcoleValideeEvent
        }
        catch (InvalidOperationException ex)
        {
            return Result<string>.Failure(ex.Message);
        }

        await _context.SaveChangesAsync(ct);

        return Result<string>.Success(
            $"École '{ecole.Nom}' ({ecole.CodeEcole}) validée et activée avec succès.");
    }
}

// ─── REJETER ÉCOLE ────────────────────────────────────────────────────────────

public class RejeterEcoleCommandHandler
    : IRequestHandler<RejeterEcoleCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public RejeterEcoleCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<string>> Handle(
        RejeterEcoleCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Motif))
            return Result<string>.Failure("Le motif de rejet est obligatoire.");

        var ecole = await _context.Ecoles
            .FirstOrDefaultAsync(e => e.Id == request.EcoleId, ct);

        if (ecole is null)
            return Result<string>.Failure("École introuvable.");

        ecole.Rejeter(request.Motif);
        await _context.SaveChangesAsync(ct);

        return Result<string>.Success($"École rejetée. Motif : {request.Motif}");
    }
}

// ─── METTRE À JOUR ÉCOLE ─────────────────────────────────────────────────────

public class MettreAJourEcoleCommandHandler
    : IRequestHandler<MettreAJourEcoleCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MettreAJourEcoleCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(
        MettreAJourEcoleCommand request, CancellationToken ct)
    {
        var ecole = await _context.Ecoles
            .FirstOrDefaultAsync(e => e.Id == request.EcoleId
                                   && e.Id == _currentUser.EcoleId, ct);

        if (ecole is null)
            return Result<string>.Failure("École introuvable ou accès non autorisé.");

        ecole.MettreAJour(
            request.Slogan,
            request.SiteWeb,
            request.TelephoneSecondaire,
            request.Email
        );

        await _context.SaveChangesAsync(ct);
        return Result<string>.Success("Informations de l'école mises à jour.");
    }
}

// ─── GET BY ID ────────────────────────────────────────────────────────────────

public class GetEcoleByIdQueryHandler
    : IRequestHandler<GetEcoleByIdQuery, Result<EcoleDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEcoleByIdQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<EcoleDetailDto>> Handle(
        GetEcoleByIdQuery request, CancellationToken ct)
    {
        var ecole = await _context.Ecoles
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EcoleId, ct);

        if (ecole is null)
            return Result<EcoleDetailDto>.Failure("École introuvable.");

        var nbrUtilisateurs = await _context.Utilisateurs
            .CountAsync(u => u.EcoleId == ecole.Id && !u.IsArchived, ct);

        var nbrEleves = await _context.Eleves
            .CountAsync(e => e.EcoleId == ecole.Id && !e.IsArchived, ct);

        return Result<EcoleDetailDto>.Success(new EcoleDetailDto(
            ecole.Id,
            ecole.Nom,
            ecole.CodeEcole,
            ecole.Type.ToString(),
            ecole.Statut.ToString(),
            ecole.Adresse,
            ecole.Ville,
            ecole.Quartier,
            ecole.Region,
            ecole.Pays,
            ecole.TelephonePrincipal,
            ecole.Email,
            ecole.SiteWeb,
            ecole.Slogan,
            ecole.LogoPath,
            ecole.NomDirecteur,
            ecole.PrenomDirecteur,
            ecole.TelephoneDirecteur,
            ecole.EmailDirecteur,
            ecole.CreatedAt,
            ecole.DateValidation,
            nbrUtilisateurs,
            nbrEleves
        ));
    }
}

// ─── GET ALL (SuperAdmin uniquement) ─────────────────────────────────────────

public class GetAllEcolesQueryHandler
    : IRequestHandler<GetAllEcolesQuery, Result<PagedResultDto<EcoleListItemDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllEcolesQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<PagedResultDto<EcoleListItemDto>>> Handle(
        GetAllEcolesQuery request, CancellationToken ct)
    {
        var query = _context.Ecoles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(e =>
                e.Nom.Contains(request.Search) ||
                e.CodeEcole.Contains(request.Search) ||
                e.Ville.Contains(request.Search));

        if (!string.IsNullOrWhiteSpace(request.Statut)
            && Enum.TryParse<StatutEcole>(request.Statut, out var statut))
            query = query.Where(e => e.Statut == statut);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new EcoleListItemDto(
                e.Id, e.Nom, e.CodeEcole,
                e.Type.ToString(), e.Statut.ToString(),
                e.Ville, e.Pays,
                e.NomDirecteur, e.TelephonePrincipal,
                e.CreatedAt))
            .ToListAsync(ct);

        return Result<PagedResultDto<EcoleListItemDto>>.Success(
            new PagedResultDto<EcoleListItemDto>(items, total, request.Page, request.PageSize));
    }
}