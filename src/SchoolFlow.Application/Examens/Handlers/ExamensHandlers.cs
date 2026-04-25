using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Examens.Handlers;

// ── QUERIES ──────────────────────────────────────────────────────────────────

public record GetAllExamensQuery(Guid? AnneeScolaireId) : IRequest<Result<List<ExamenDto>>>;

public class GetAllExamensQueryHandler
    : IRequestHandler<GetAllExamensQuery, Result<List<ExamenDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAllExamensQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<ExamenDto>>> Handle(
        GetAllExamensQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        if (ecoleId == Guid.Empty)
            return Result<List<ExamenDto>>.Failure("Contexte école manquant.");

        var query = _context.Examens
            .Include(e => e.Inscrits)
            .Where(e => e.EcoleId == ecoleId);

        if (request.AnneeScolaireId.HasValue)
            query = query.Where(e => e.AnneeScolaireId == request.AnneeScolaireId.Value);

        var examens = await query
            .OrderByDescending(e => e.DateDebut)
            .Select(e => new ExamenDto(
                e.Id, e.Nom, e.Type.ToString(),
                e.DateDebut, e.DateFin,
                e.Inscrits.Count))
            .ToListAsync(ct);

        return Result<List<ExamenDto>>.Success(examens);
    }
}

public record GetResultatsExamenQuery(Guid ExamenId) : IRequest<Result<List<ResultatExamenDto>>>;

public class GetResultatsExamenQueryHandler
    : IRequestHandler<GetResultatsExamenQuery, Result<List<ResultatExamenDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetResultatsExamenQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<ResultatExamenDto>>> Handle(
        GetResultatsExamenQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var inscrits = await _context.InscriptionsExamen
            .Include(i => i.Eleve)
            .Where(i => i.ExamenId == request.ExamenId && i.EcoleId == ecoleId)
            .OrderBy(i => i.Eleve.Nom)
            .ToListAsync(ct);

        var dtos = inscrits.Select(i => new ResultatExamenDto(
            i.EleveId, i.Eleve.NomComplet, i.Eleve.Matricule,
            i.NumeroCandidat, i.Admis, i.MoyenneExamen, i.Mention
        )).ToList();

        return Result<List<ResultatExamenDto>>.Success(dtos);
    }
}

// ── COMMANDS ─────────────────────────────────────────────────────────────────

public record CreateExamenCommand(
    Guid AnneeScolaireId, string Nom, string Type,
    DateTime DateDebut, DateTime DateFin, string? Description
) : IRequest<Result<Guid>>;

public class CreateExamenCommandHandler
    : IRequestHandler<CreateExamenCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateExamenCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateExamenCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        if (ecoleId == Guid.Empty)
            return Result<Guid>.Failure("Contexte école manquant.");

        if (!Enum.TryParse<TypeExamen>(request.Type, out var type))
            return Result<Guid>.Failure("Type d'examen invalide.");

        var examen = new Examen
        {
            EcoleId = ecoleId,
            AnneeScolaireId = request.AnneeScolaireId,
            Nom = request.Nom,
            Type = type,
            DateDebut = request.DateDebut,
            DateFin = request.DateFin,
            Description = request.Description
        };

        _context.Examens.Add(examen);
        await _context.SaveChangesAsync(ct);
        return Result<Guid>.Success(examen.Id);
    }
}

public record InscrireEleveExamenCommand(
    Guid ExamenId, List<Guid> EleveIds
) : IRequest<Result<int>>;

public class InscrireEleveExamenCommandHandler
    : IRequestHandler<InscrireEleveExamenCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public InscrireEleveExamenCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(InscrireEleveExamenCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var examen = await _context.Examens
            .FirstOrDefaultAsync(e => e.Id == request.ExamenId && e.EcoleId == ecoleId, ct);
        if (examen is null)
            return Result<int>.Failure("Examen introuvable.");

        int inscrits = 0;
        foreach (var eleveId in request.EleveIds)
        {
            var existe = await _context.InscriptionsExamen
                .AnyAsync(i => i.ExamenId == request.ExamenId && i.EleveId == eleveId, ct);
            if (existe) continue;

            _context.InscriptionsExamen.Add(new InscriptionExamen
            {
                EcoleId = ecoleId,
                ExamenId = request.ExamenId,
                EleveId = eleveId,
                Admis = false
            });
            inscrits++;
        }

        if (inscrits > 0) await _context.SaveChangesAsync(ct);
        return Result<int>.Success(inscrits);
    }
}

public record SaisirResultatExamenCommand(
    Guid InscriptionId, bool Admis,
    decimal? MoyenneExamen, string? Mention
) : IRequest<Result<bool>>;

public class SaisirResultatExamenCommandHandler
    : IRequestHandler<SaisirResultatExamenCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SaisirResultatExamenCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(SaisirResultatExamenCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        var inscription = await _context.InscriptionsExamen
            .FirstOrDefaultAsync(i => i.Id == request.InscriptionId && i.EcoleId == ecoleId, ct);

        if (inscription is null)
            return Result<bool>.Failure("Inscription introuvable.");

        inscription.Admis = request.Admis;
        inscription.MoyenneExamen = request.MoyenneExamen;
        inscription.Mention = request.Mention;
        inscription.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
