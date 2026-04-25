using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Enseignants.Handlers;

// ── QUERIES ──────────────────────────────────────────────────────────────────

public record GetAllEnseignantsQuery : IRequest<Result<List<EnseignantDto>>>;

public class GetAllEnseignantsQueryHandler
    : IRequestHandler<GetAllEnseignantsQuery, Result<List<EnseignantDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAllEnseignantsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<EnseignantDto>>> Handle(
        GetAllEnseignantsQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        if (ecoleId == Guid.Empty)
            return Result<List<EnseignantDto>>.Failure("Contexte école manquant.");

        var enseignants = await _context.Enseignants
            .Include(e => e.Utilisateur)
            .Include(e => e.Matieres).ThenInclude(me => me.Matiere)
            .Where(e => e.EcoleId == ecoleId)
            .OrderBy(e => e.Utilisateur.Nom)
            .ToListAsync(ct);

        var dtos = enseignants.Select(e => new EnseignantDto(
            e.Id,
            e.Utilisateur.NomComplet,
            e.Telephone,
            e.Specialite,
            e.Grade,
            e.IsActive,
            e.Matieres.Select(me => me.Matiere.Libelle).Distinct().ToList()
        )).ToList();

        return Result<List<EnseignantDto>>.Success(dtos);
    }
}

public record GetEnseignantByIdQuery(Guid Id) : IRequest<Result<EnseignantDto>>;

public class GetEnseignantByIdQueryHandler
    : IRequestHandler<GetEnseignantByIdQuery, Result<EnseignantDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetEnseignantByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<EnseignantDto>> Handle(
        GetEnseignantByIdQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        var e = await _context.Enseignants
            .Include(x => x.Utilisateur)
            .Include(x => x.Matieres).ThenInclude(me => me.Matiere)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.EcoleId == ecoleId, ct);

        if (e is null)
            return Result<EnseignantDto>.Failure("Enseignant introuvable.");

        return Result<EnseignantDto>.Success(new EnseignantDto(
            e.Id, e.Utilisateur.NomComplet, e.Telephone,
            e.Specialite, e.Grade, e.IsActive,
            e.Matieres.Select(me => me.Matiere.Libelle).Distinct().ToList()
        ));
    }
}

// ── COMMANDS ─────────────────────────────────────────────────────────────────

public record CreateEnseignantCommand(
    Guid UtilisateurId, string Telephone,
    string? Specialite, string? Grade, int? AnneesExperience, string? Bio
) : IRequest<Result<Guid>>;

public class CreateEnseignantCommandHandler
    : IRequestHandler<CreateEnseignantCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateEnseignantCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateEnseignantCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        if (ecoleId == Guid.Empty)
            return Result<Guid>.Failure("Contexte école manquant.");

        var utilisateurExiste = await _context.Utilisateurs
            .AnyAsync(u => u.Id == request.UtilisateurId, ct);
        if (!utilisateurExiste)
            return Result<Guid>.Failure("Utilisateur introuvable.");

        var dejaEnseignant = await _context.Enseignants
            .AnyAsync(e => e.UtilisateurId == request.UtilisateurId && e.EcoleId == ecoleId, ct);
        if (dejaEnseignant)
            return Result<Guid>.Failure("Cet utilisateur est déjà enregistré comme enseignant.");

        var enseignant = new Enseignant
        {
            EcoleId = ecoleId,
            UtilisateurId = request.UtilisateurId,
            Telephone = request.Telephone,
            Specialite = request.Specialite,
            Grade = request.Grade,
            AnneesExperience = request.AnneesExperience,
            Bio = request.Bio,
            IsActive = true
        };

        _context.Enseignants.Add(enseignant);
        await _context.SaveChangesAsync(ct);
        return Result<Guid>.Success(enseignant.Id);
    }
}

public record AssignerMatiereCommand(
    Guid EnseignantId, Guid MatiereId,
    Guid ClasseId, Guid AnneeScolaireId
) : IRequest<Result<bool>>;

public class AssignerMatiereCommandHandler
    : IRequestHandler<AssignerMatiereCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AssignerMatiereCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(AssignerMatiereCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var enseignant = await _context.Enseignants
            .FirstOrDefaultAsync(e => e.Id == request.EnseignantId && e.EcoleId == ecoleId, ct);
        if (enseignant is null)
            return Result<bool>.Failure("Enseignant introuvable.");

        var existe = await _context.MatiereEnseignants
            .AnyAsync(me => me.EnseignantId == request.EnseignantId
                         && me.MatiereId == request.MatiereId
                         && me.ClasseId == request.ClasseId
                         && me.AnneeScolaireId == request.AnneeScolaireId, ct);
        if (existe)
            return Result<bool>.Failure("Cette assignation existe déjà.");

        _context.MatiereEnseignants.Add(new MatiereEnseignant
        {
            EcoleId = ecoleId,
            EnseignantId = request.EnseignantId,
            MatiereId = request.MatiereId,
            ClasseId = request.ClasseId,
            AnneeScolaireId = request.AnneeScolaireId
        });

        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
