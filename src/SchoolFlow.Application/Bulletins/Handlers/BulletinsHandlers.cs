using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Bulletins.Handlers;

// ── GÉNÉRATION ────────────────────────────────────────────────────────────────

public record GenererBulletinsCommand(
    Guid ClasseId, Guid PeriodeId, Guid AnneeScolaireId
) : IRequest<Result<int>>;

public class GenererBulletinsCommandHandler
    : IRequestHandler<GenererBulletinsCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GenererBulletinsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(GenererBulletinsCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        if (ecoleId == Guid.Empty)
            return Result<int>.Failure("Contexte école manquant.");

        var eleves = await _context.Eleves
            .Where(e => e.ClasseId == request.ClasseId
                     && e.EcoleId == ecoleId
                     && e.AnneeScolaireId == request.AnneeScolaireId
                     && !e.IsArchived)
            .ToListAsync(ct);

        if (!eleves.Any())
            return Result<int>.Failure("Aucun élève dans cette classe.");

        var matieres = await _context.Matieres
            .Where(m => m.EcoleId == ecoleId && m.IsActive)
            .ToListAsync(ct);

        var notes = await _context.Notes
            .Where(n => n.EcoleId == ecoleId
                     && n.PeriodeId == request.PeriodeId
                     && n.EstPubliee)
            .ToListAsync(ct);

        int bulletinsGeneres = 0;

        foreach (var eleve in eleves)
        {
            var ancienBulletin = await _context.Bulletins
                .Include(b => b.Lignes)
                .FirstOrDefaultAsync(b =>
                    b.EleveId == eleve.Id &&
                    b.PeriodeId == request.PeriodeId &&
                    b.AnneeScolaireId == request.AnneeScolaireId, ct);

            if (ancienBulletin is not null)
            {
                _context.Bulletins.Remove(ancienBulletin);
                await _context.SaveChangesAsync(ct);
            }

            var notesEleve = notes.Where(n => n.EleveId == eleve.Id).ToList();
            if (!notesEleve.Any()) continue;

            var lignes = new List<LigneBulletin>();
            decimal totalPondere = 0;
            decimal totalCoefficients = 0;

            foreach (var matiere in matieres)
            {
                var notesMat = notesEleve.Where(n => n.MatiereId == matiere.Id).ToList();
                if (!notesMat.Any()) continue;

                var moyenneMatiere = Math.Round(notesMat.Average(n => n.ValeurSur20), 2);
                var moyennePonderee = Math.Round(moyenneMatiere * matiere.Coefficient, 2);

                var notesClasseMat = notes
                    .Where(n => n.MatiereId == matiere.Id)
                    .Select(n => n.ValeurSur20)
                    .ToList();

                var moyenneClasse = notesClasseMat.Any()
                    ? Math.Round(notesClasseMat.Average(), 2)
                    : (decimal?)null;

                lignes.Add(new LigneBulletin
                {
                    EcoleId = ecoleId,
                    MatiereId = matiere.Id,
                    MoyenneMatiere = moyenneMatiere,
                    Coefficient = matiere.Coefficient,
                    MoyennePonderee = moyennePonderee,
                    MoyenneClasse = moyenneClasse
                });

                totalPondere += moyennePonderee;
                totalCoefficients += matiere.Coefficient;
            }

            if (!lignes.Any()) continue;

            var moyenneGenerale = totalCoefficients > 0
                ? Math.Round(totalPondere / totalCoefficients, 2)
                : 0;

            var bulletin = new Bulletin
            {
                EcoleId = ecoleId,
                EleveId = eleve.Id,
                ClasseId = request.ClasseId,
                PeriodeId = request.PeriodeId,
                AnneeScolaireId = request.AnneeScolaireId,
                MoyenneGenerale = moyenneGenerale,
                EffectifClasse = eleves.Count,
                Lignes = lignes
            };

            _context.Bulletins.Add(bulletin);
            bulletinsGeneres++;
        }

        await _context.SaveChangesAsync(ct);

        // Calcul des rangs
        var bulletinsClasse = await _context.Bulletins
            .Where(b => b.ClasseId == request.ClasseId
                     && b.PeriodeId == request.PeriodeId
                     && b.AnneeScolaireId == request.AnneeScolaireId)
            .OrderByDescending(b => b.MoyenneGenerale)
            .ToListAsync(ct);

        for (int i = 0; i < bulletinsClasse.Count; i++)
            bulletinsClasse[i].RangClasse = i + 1;

        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(bulletinsGeneres);
    }
}

// ── QUERIES ──────────────────────────────────────────────────────────────────

public record GetBulletinsClasseQuery(
    Guid ClasseId, Guid PeriodeId, Guid AnneeScolaireId
) : IRequest<Result<List<BulletinResumeDto>>>;

public class GetBulletinsClasseQueryHandler
    : IRequestHandler<GetBulletinsClasseQuery, Result<List<BulletinResumeDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetBulletinsClasseQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<BulletinResumeDto>>> Handle(
        GetBulletinsClasseQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var bulletins = await _context.Bulletins
            .Include(b => b.Eleve)
            .Where(b => b.EcoleId == ecoleId
                     && b.ClasseId == request.ClasseId
                     && b.PeriodeId == request.PeriodeId
                     && b.AnneeScolaireId == request.AnneeScolaireId)
            .OrderBy(b => b.RangClasse)
            .ToListAsync(ct);

        var dtos = bulletins.Select(b => new BulletinResumeDto(
            b.Id, b.Eleve.NomComplet, b.Eleve.Matricule,
            b.MoyenneGenerale, b.RangClasse, b.EffectifClasse,
            b.Appreciation, b.EstPublie
        )).ToList();

        return Result<List<BulletinResumeDto>>.Success(dtos);
    }
}

public record GetBulletinsEleveQuery(Guid EleveId) : IRequest<Result<List<BulletinResumeDto>>>;

public class GetBulletinsEleveQueryHandler
    : IRequestHandler<GetBulletinsEleveQuery, Result<List<BulletinResumeDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetBulletinsEleveQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<BulletinResumeDto>>> Handle(
        GetBulletinsEleveQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var bulletins = await _context.Bulletins
            .Include(b => b.Eleve)
            .Where(b => b.EcoleId == ecoleId && b.EleveId == request.EleveId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

        var dtos = bulletins.Select(b => new BulletinResumeDto(
            b.Id, b.Eleve.NomComplet, b.Eleve.Matricule,
            b.MoyenneGenerale, b.RangClasse, b.EffectifClasse,
            b.Appreciation, b.EstPublie
        )).ToList();

        return Result<List<BulletinResumeDto>>.Success(dtos);
    }
}

public record GetBulletinDetailQuery(Guid Id) : IRequest<Result<BulletinDetailDto>>;

public class GetBulletinDetailQueryHandler
    : IRequestHandler<GetBulletinDetailQuery, Result<BulletinDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetBulletinDetailQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<BulletinDetailDto>> Handle(
        GetBulletinDetailQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        var b = await _context.Bulletins
            .Include(x => x.Eleve)
            .Include(x => x.Classe)
            .Include(x => x.Periode)
            .Include(x => x.Lignes).ThenInclude(l => l.Matiere)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.EcoleId == ecoleId, ct);

        if (b is null)
            return Result<BulletinDetailDto>.Failure("Bulletin introuvable.");

        var annee = await _context.AnneeScolaires
            .FirstOrDefaultAsync(a => a.Id == b.AnneeScolaireId, ct);

        var lignesDto = b.Lignes
            .OrderBy(l => l.Matiere.Code)
            .Select(l => new LigneBulletinDto(
                l.Matiere.Code, l.Matiere.Libelle,
                l.Coefficient, l.MoyenneMatiere,
                l.MoyennePonderee, l.MoyenneClasse,
                l.MoyenneMatiere switch
                {
                    >= 16 => "Très Bien", >= 14 => "Bien",
                    >= 12 => "Assez Bien", >= 10 => "Passable",
                    >= 8  => "Insuffisant", _ => "Médiocre"
                },
                l.AppreciationEnseignant
            )).ToList();

        var dto = new BulletinDetailDto(
            b.Id,
            b.Eleve.NomComplet, b.Eleve.Matricule,
            b.Classe.NomComplet, b.Periode.Libelle,
            annee?.Libelle ?? "",
            b.MoyenneGenerale, b.RangClasse, b.EffectifClasse,
            b.Appreciation, b.EstPublie,
            lignesDto,
            b.AppreciationProfesseur,
            b.AppreciationDirecteur
        );

        return Result<BulletinDetailDto>.Success(dto);
    }
}

// ── PUBLICATION / APPRECIATION ────────────────────────────────────────────────

public record PublierBulletinCommand(Guid Id) : IRequest<Result<bool>>;

public class PublierBulletinCommandHandler
    : IRequestHandler<PublierBulletinCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PublierBulletinCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(PublierBulletinCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        var bulletin = await _context.Bulletins
            .FirstOrDefaultAsync(b => b.Id == request.Id && b.EcoleId == ecoleId, ct);

        if (bulletin is null) return Result<bool>.Failure("Bulletin introuvable.");
        bulletin.Publier();
        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}

public record AjouterAppreciationDirecteurCommand(
    Guid Id, string Appreciation
) : IRequest<Result<bool>>;

public class AjouterAppreciationDirecteurCommandHandler
    : IRequestHandler<AjouterAppreciationDirecteurCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AjouterAppreciationDirecteurCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(AjouterAppreciationDirecteurCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        var bulletin = await _context.Bulletins
            .FirstOrDefaultAsync(b => b.Id == request.Id && b.EcoleId == ecoleId, ct);

        if (bulletin is null) return Result<bool>.Failure("Bulletin introuvable.");
        bulletin.AppreciationDirecteur = request.Appreciation;
        bulletin.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
