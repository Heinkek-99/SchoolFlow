using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Eleves.Commands;

public record CreateEleveCommand : IRequest<Result<CreateEleveResponse>>
{
    public string Nom { get; init; } = string.Empty;
    public string Prenom { get; init; } = string.Empty;
    public DateTime DateNaissance { get; init; }
    public string LieuNaissance { get; init; } = string.Empty;
    public Sexe Sexe { get; init; }
    public Guid FamilleId { get; init; }
    public Guid ClasseId { get; init; }
    public string? PhotoPath { get; init; }
}

public record CreateEleveResponse(Guid EleveId, string Matricule);

public class CreateEleveCommandValidator : AbstractValidator<CreateEleveCommand>
{
    public CreateEleveCommandValidator()
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Prenom).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateNaissance)
            .NotEmpty()
            .LessThan(DateTime.Today.AddYears(-3)).WithMessage("L'élève doit avoir au moins 3 ans")
            .GreaterThan(DateTime.Today.AddYears(-25)).WithMessage("L'élève doit avoir moins de 25 ans");
        RuleFor(x => x.LieuNaissance).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FamilleId).NotEmpty();
        RuleFor(x => x.ClasseId).NotEmpty();
    }
}

public class CreateEleveCommandHandler : IRequestHandler<CreateEleveCommand, Result<CreateEleveResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreateEleveCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreateEleveResponse>> Handle(CreateEleveCommand request, CancellationToken ct)
    {
        // Vérifier famille existe
        var familleExists = await _context.Familles.AnyAsync(f => f.Id == request.FamilleId, ct);
        if (!familleExists)
            return Result<CreateEleveResponse>.Failure("Famille introuvable");

        // Vérifier classe existe
        var classe = await _context.Classes
            .Include(c => c.AnneeScolaire)
            .FirstOrDefaultAsync(c => c.Id == request.ClasseId, ct);
        
        if (classe == null)
            return Result<CreateEleveResponse>.Failure("Classe introuvable");

        // Générer matricule unique
        var matricule = await GenerateMatriculeAsync(ct);

        var eleve = new Eleve
        {
            Matricule = matricule,
            Nom = request.Nom,
            Prenom = request.Prenom,
            DateNaissance = request.DateNaissance,
            LieuNaissance = request.LieuNaissance,
            Sexe = request.Sexe,
            FamilleId = request.FamilleId,
            ClasseId = request.ClasseId,
            AnneeScolaireId = classe.AnneeScolaireId,
            PhotoPath = request.PhotoPath,
            Statut = StatutEleve.Actif,
            DateInscription = DateTime.UtcNow
        };

        _context.Eleves.Add(eleve);
        await _context.SaveChangesAsync(ct);

        // Générer frais automatiques
        await GenererFraisAutomatiquesAsync(eleve.Id, classe.Niveau, ct);

        return Result<CreateEleveResponse>.Success(
            new CreateEleveResponse(eleve.Id, matricule)
        );
    }

    private async Task<string> GenerateMatriculeAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var lastMatricule = await _context.Eleves
            .Where(e => e.Matricule.StartsWith($"EL{year}"))
            .OrderByDescending(e => e.Matricule)
            .Select(e => e.Matricule)
            .FirstOrDefaultAsync(ct);

        int sequence = 1;
        if (lastMatricule != null)
        {
            var lastSeq = lastMatricule.Substring(6); // "EL2025-00123" -> "00123"
            if (int.TryParse(lastSeq, out int num))
                sequence = num + 1;
        }

        return $"EL{year}-{sequence:D5}"; // EL2025-00001
    }

    private async Task GenererFraisAutomatiquesAsync(Guid eleveId, Niveau niveau, CancellationToken ct)
    {
        var typesFraisAuto = await _context.TypeFrais
            .Where(t => t.GenerationAutomatique && !t.IsArchived)
            .ToListAsync(ct);

        var anneeScolaire = await _context.AnneeScolaires
            .Include(a => a.Periodes)
            .FirstOrDefaultAsync(a => a.IsActive, ct);

        if (anneeScolaire == null) return;

        var fraisListe = new List<Frais>();

        foreach (var typeFrais in typesFraisAuto)
        {
            if (!typeFrais.MontantsParNiveau.TryGetValue(niveau, out decimal montant))
                continue;

            if (typeFrais.Categorie == CategorieFrais.Inscription)
            {
                // Frais unique
                fraisListe.Add(new Frais
                {
                    EleveId = eleveId,
                    TypeFraisId = typeFrais.Id,
                    Montant = montant,
                    DateEcheance = DateTime.UtcNow.AddDays(30)
                });
            }
            else if (typeFrais.Categorie == CategorieFrais.Scolarite)
            {
                // Frais trimestriels
                var trimestres = anneeScolaire.Periodes
                    .Where(p => p.Type == TypePeriode.Trimestre)
                    .OrderBy(p => p.Numero)
                    .ToList();

                foreach (var trimestre in trimestres)
                {
                    fraisListe.Add(new Frais
                    {
                        EleveId = eleveId,
                        TypeFraisId = typeFrais.Id,
                        PeriodeId = trimestre.Id,
                        Montant = montant / 3, // 90K / 3 = 30K par trimestre
                        DateEcheance = trimestre.DateFin.AddDays(-15)
                    });
                }
            }
        }

        _context.Frais.AddRange(fraisListe);
        await _context.SaveChangesAsync(ct);
    }
}