using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Evaluations.Handlers;

// ── QUERIES ──────────────────────────────────────────────────────────────────

public record GetAllEvaluationsQuery(
    Guid? ClasseId, Guid? PeriodeId, Guid? AnneeScolaireId
) : IRequest<Result<List<EvaluationDto>>>;

public class GetAllEvaluationsQueryHandler
    : IRequestHandler<GetAllEvaluationsQuery, Result<List<EvaluationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAllEvaluationsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<EvaluationDto>>> Handle(
        GetAllEvaluationsQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        if (ecoleId == Guid.Empty)
            return Result<List<EvaluationDto>>.Failure("Contexte école manquant.");

        var query = _context.Evaluations
            .Include(e => e.Matiere)
            .Include(e => e.Classe)
            .Include(e => e.Periode)
            .Where(e => e.EcoleId == ecoleId);

        if (request.ClasseId.HasValue)
            query = query.Where(e => e.ClasseId == request.ClasseId.Value);
        if (request.PeriodeId.HasValue)
            query = query.Where(e => e.PeriodeId == request.PeriodeId.Value);
        if (request.AnneeScolaireId.HasValue)
            query = query.Where(e => e.AnneeScolaireId == request.AnneeScolaireId.Value);

        var evaluations = await query
            .OrderByDescending(e => e.DateEvaluation)
            .ToListAsync(ct);

        var notesCounts = await _context.Notes
            .Where(n => n.EcoleId == ecoleId && n.EvaluationId != null)
            .GroupBy(n => n.EvaluationId)
            .Select(g => new { EvalId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var countDict = notesCounts.ToDictionary(x => x.EvalId!, x => x.Count);

        var dtos = evaluations.Select(e => new EvaluationDto(
            e.Id,
            e.Type.ToString(),
            e.DateEvaluation,
            e.Matiere.Libelle,
            e.Classe.NomComplet,
            e.Periode.Libelle,
            e.NoteSur,
            e.EstPubliee,
            countDict.TryGetValue(e.Id, out var c) ? c : 0
        )).ToList();

        return Result<List<EvaluationDto>>.Success(dtos);
    }
}

public record GetNotesEvaluationQuery(Guid EvaluationId) : IRequest<Result<List<NoteDto>>>;

public class GetNotesEvaluationQueryHandler
    : IRequestHandler<GetNotesEvaluationQuery, Result<List<NoteDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetNotesEvaluationQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<NoteDto>>> Handle(
        GetNotesEvaluationQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        var notes = await _context.Notes
            .Include(n => n.Eleve)
            .Where(n => n.EvaluationId == request.EvaluationId && n.EcoleId == ecoleId)
            .OrderBy(n => n.Eleve.Nom)
            .ToListAsync(ct);

        var dtos = notes.Select(n => new NoteDto(
            n.Id,
            n.Eleve.NomComplet,
            n.Eleve.Matricule,
            n.Valeur,
            n.NoteSur,
            n.ValeurSur20,
            n.Appreciation,
            n.Commentaire
        )).ToList();

        return Result<List<NoteDto>>.Success(dtos);
    }
}

public record GetNotesEleveQuery(Guid EleveId, Guid? PeriodeId) : IRequest<Result<List<NoteDto>>>;

public class GetNotesEleveQueryHandler
    : IRequestHandler<GetNotesEleveQuery, Result<List<NoteDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetNotesEleveQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<NoteDto>>> Handle(
        GetNotesEleveQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        var query = _context.Notes
            .Include(n => n.Eleve)
            .Include(n => n.Matiere)
            .Where(n => n.EleveId == request.EleveId && n.EcoleId == ecoleId && n.EstPubliee);

        if (request.PeriodeId.HasValue)
            query = query.Where(n => n.PeriodeId == request.PeriodeId.Value);

        var notes = await query.OrderBy(n => n.Matiere.Code).ToListAsync(ct);

        var dtos = notes.Select(n => new NoteDto(
            n.Id, n.Eleve.NomComplet, n.Eleve.Matricule,
            n.Valeur, n.NoteSur, n.ValeurSur20, n.Appreciation, n.Commentaire
        )).ToList();

        return Result<List<NoteDto>>.Success(dtos);
    }
}

// ── COMMANDS ─────────────────────────────────────────────────────────────────

public record CreateEvaluationCommand(
    Guid ClasseId, Guid MatiereId, Guid PeriodeId, Guid AnneeScolaireId,
    string Type, DateTime DateEvaluation, string? Description, decimal NoteSur
) : IRequest<Result<Guid>>;

public class CreateEvaluationCommandHandler
    : IRequestHandler<CreateEvaluationCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateEvaluationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateEvaluationCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        if (ecoleId == Guid.Empty)
            return Result<Guid>.Failure("Contexte école manquant.");

        if (!Enum.TryParse<TypeEvaluation>(request.Type, out var type))
            return Result<Guid>.Failure("Type d'évaluation invalide.");

        var eval = new EvaluationPlanifiee
        {
            EcoleId = ecoleId,
            ClasseId = request.ClasseId,
            MatiereId = request.MatiereId,
            PeriodeId = request.PeriodeId,
            AnneeScolaireId = request.AnneeScolaireId,
            Type = type,
            DateEvaluation = request.DateEvaluation,
            Description = request.Description,
            NoteSur = request.NoteSur
        };

        _context.Evaluations.Add(eval);
        await _context.SaveChangesAsync(ct);
        return Result<Guid>.Success(eval.Id);
    }
}

public record NoteSaisie(Guid EleveId, decimal Valeur, string? Commentaire);

public record SaisirNotesCommand(
    Guid EvaluationId, List<NoteSaisie> Notes
) : IRequest<Result<int>>;

public class SaisirNotesCommandHandler
    : IRequestHandler<SaisirNotesCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SaisirNotesCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(SaisirNotesCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        if (ecoleId == Guid.Empty)
            return Result<int>.Failure("Contexte école manquant.");

        var evaluation = await _context.Evaluations
            .FirstOrDefaultAsync(e => e.Id == request.EvaluationId && e.EcoleId == ecoleId, ct);

        if (evaluation is null)
            return Result<int>.Failure("Évaluation introuvable.");

        if (evaluation.EstPubliee)
            return Result<int>.Failure("Impossible de modifier une évaluation publiée.");

        int saisies = 0;
        foreach (var n in request.Notes)
        {
            if (n.Valeur < 0 || n.Valeur > evaluation.NoteSur)
                return Result<int>.Failure(
                    $"Note {n.Valeur} invalide (max {evaluation.NoteSur}).");

            var noteExistante = await _context.Notes
                .FirstOrDefaultAsync(x =>
                    x.EleveId == n.EleveId &&
                    x.EvaluationId == request.EvaluationId, ct);

            if (noteExistante is not null)
            {
                noteExistante.Valeur = n.Valeur;
                noteExistante.Commentaire = n.Commentaire;
                noteExistante.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.Notes.Add(new Note
                {
                    EcoleId = ecoleId,
                    EleveId = n.EleveId,
                    MatiereId = evaluation.MatiereId,
                    PeriodeId = evaluation.PeriodeId,
                    EvaluationId = evaluation.Id,
                    Valeur = n.Valeur,
                    NoteSur = evaluation.NoteSur,
                    Type = evaluation.Type,
                    Commentaire = n.Commentaire
                });
            }
            saisies++;
        }

        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(saisies);
    }
}

public record PublierEvaluationCommand(Guid EvaluationId) : IRequest<Result<bool>>;

public class PublierEvaluationCommandHandler
    : IRequestHandler<PublierEvaluationCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PublierEvaluationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(PublierEvaluationCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        var evaluation = await _context.Evaluations
            .Include(e => e.Notes)
            .FirstOrDefaultAsync(e => e.Id == request.EvaluationId && e.EcoleId == ecoleId, ct);

        if (evaluation is null)
            return Result<bool>.Failure("Évaluation introuvable.");

        if (evaluation.EstPubliee)
            return Result<bool>.Failure("Évaluation déjà publiée.");

        evaluation.Publier();
        foreach (var note in evaluation.Notes)
            note.Publier();

        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
