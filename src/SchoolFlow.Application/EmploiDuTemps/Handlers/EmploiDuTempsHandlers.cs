using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.EmploiDuTemps.Handlers;

// ── QUERIES ──────────────────────────────────────────────────────────────────

public record GetEmploiDuTempsClasseQuery(Guid ClasseId, Guid AnneeScolaireId)
    : IRequest<Result<EmploiDuTempsClasseDto>>;

public class GetEmploiDuTempsClasseQueryHandler
    : IRequestHandler<GetEmploiDuTempsClasseQuery, Result<EmploiDuTempsClasseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetEmploiDuTempsClasseQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<EmploiDuTempsClasseDto>> Handle(
        GetEmploiDuTempsClasseQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        var classe = await _context.Classes
            .FirstOrDefaultAsync(c => c.Id == request.ClasseId && c.EcoleId == ecoleId, ct);

        if (classe is null)
            return Result<EmploiDuTempsClasseDto>.Failure("Classe introuvable.");

        var creneaux = await _context.CreneauxHoraires
            .Include(c => c.Matiere)
            .Include(c => c.Enseignant).ThenInclude(e => e!.Utilisateur)
            .Where(c => c.ClasseId == request.ClasseId
                     && c.AnneeScolaireId == request.AnneeScolaireId
                     && c.EcoleId == ecoleId)
            .OrderBy(c => c.Jour).ThenBy(c => c.HeureDebut)
            .ToListAsync(ct);

        var planningParJour = creneaux
            .GroupBy(c => c.Jour.ToString())
            .ToDictionary(
                g => g.Key,
                g => g.Select(c => new CreneauDto(
                    c.Id,
                    c.Jour.ToString(),
                    c.HeureDebut.ToString("HH:mm"),
                    c.HeureFin.ToString("HH:mm"),
                    c.Matiere.Libelle,
                    c.Enseignant?.Utilisateur.NomComplet,
                    c.Salle
                )).ToList()
            );

        return Result<EmploiDuTempsClasseDto>.Success(
            new EmploiDuTempsClasseDto(classe.Id, classe.NomComplet, planningParJour));
    }
}

// ── COMMANDS ─────────────────────────────────────────────────────────────────

public record CreateCreneauCommand(
    Guid ClasseId, Guid MatiereId, Guid AnneeScolaireId,
    Guid? EnseignantId, string Jour,
    string HeureDebut, string HeureFin,
    string? Salle, string? Remarque
) : IRequest<Result<Guid>>;

public class CreateCreneauCommandHandler
    : IRequestHandler<CreateCreneauCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateCreneauCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateCreneauCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        if (ecoleId == Guid.Empty)
            return Result<Guid>.Failure("Contexte école manquant.");

        if (!Enum.TryParse<JourSemaine>(request.Jour, out var jour))
            return Result<Guid>.Failure("Jour invalide.");

        if (!TimeOnly.TryParse(request.HeureDebut, out var debut))
            return Result<Guid>.Failure("Heure de début invalide (format HH:mm).");

        if (!TimeOnly.TryParse(request.HeureFin, out var fin))
            return Result<Guid>.Failure("Heure de fin invalide (format HH:mm).");

        if (fin <= debut)
            return Result<Guid>.Failure("L'heure de fin doit être après l'heure de début.");

        var creneau = new CreneauHoraire
        {
            EcoleId = ecoleId,
            ClasseId = request.ClasseId,
            MatiereId = request.MatiereId,
            AnneeScolaireId = request.AnneeScolaireId,
            EnseignantId = request.EnseignantId,
            Jour = jour,
            HeureDebut = debut,
            HeureFin = fin,
            Salle = request.Salle,
            Remarque = request.Remarque
        };

        _context.CreneauxHoraires.Add(creneau);
        await _context.SaveChangesAsync(ct);
        return Result<Guid>.Success(creneau.Id);
    }
}

public record SupprimerCreneauCommand(Guid Id) : IRequest<Result<bool>>;

public class SupprimerCreneauCommandHandler
    : IRequestHandler<SupprimerCreneauCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SupprimerCreneauCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(SupprimerCreneauCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        var creneau = await _context.CreneauxHoraires
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.EcoleId == ecoleId, ct);

        if (creneau is null) return Result<bool>.Failure("Créneau introuvable.");
        _context.CreneauxHoraires.Remove(creneau);
        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
