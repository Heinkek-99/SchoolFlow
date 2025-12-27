namespace SchoolFlow.Application.Eleves.Handlers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Eleves.Commands;
using SchoolFlow.Domain.Entities;

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

        // Vérifier si le matricule existe déjà
        if (await _context.Eleves.AnyAsync(e => e.Matricule == matricule, ct))
            return Result<CreateEleveResponse>.Failure("Le matricule généré existe déjà.");

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
        int sequence = 1;

        var lastMatricule = await _context.Eleves
            .Where(e => e.Matricule.StartsWith($"EL{year}"))
            .OrderByDescending(e => e.Matricule)
            .Select(e => e.Matricule)
            .FirstOrDefaultAsync(ct);

        if (lastMatricule != null)
        {
            var lastSeq = lastMatricule.Substring(6); // "EL2025-00123" -> "00123"
            if (int.TryParse(lastSeq, out int num))
                sequence = num + 1;
        }
        
        string newMatricule;
        do
        {
            newMatricule = $"EL{year}-{sequence:D5}"; // EL2025-00001
            sequence++;
        } while (await _context.Eleves.AnyAsync(e => e.Matricule == newMatricule, ct));

        return newMatricule;

        // return $"EL{year}-{sequence:D5}"; // EL2025-00001
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