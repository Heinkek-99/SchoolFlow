using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.AnneeScolaires.Commands;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.AnneeScolaires.Handlers;

public class CreateAnneeScolaireCommandHandler : IRequestHandler<CreateAnneeScolaireCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateAnneeScolaireCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateAnneeScolaireCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        if (ecoleId == Guid.Empty)
            return Result<Guid>.Failure("Contexte école manquant — reconnectez-vous.");

        if (request.DateFin <= request.DateDebut)
            return Result<Guid>.Failure("La date de fin doit être postérieure à la date de début.");

        var libelleExiste = await _context.AnneeScolaires
            .AnyAsync(a => a.EcoleId == ecoleId && a.Libelle == request.Libelle, ct);

        if (libelleExiste)
            return Result<Guid>.Failure($"Une année scolaire '{request.Libelle}' existe déjà.");

        // Si activation immédiate, désactiver toutes les autres
        if (request.ActiverImmediatement)
        {
            var anneesActives = await _context.AnneeScolaires
                .Where(a => a.EcoleId == ecoleId && a.IsActive)
                .ToListAsync(ct);

            foreach (var a in anneesActives)
                a.Desactiver();
        }

        var annee = new AnneeScolaire
        {
            EcoleId = ecoleId,
            Libelle = request.Libelle.Trim(),
            DateDebut = DateTime.SpecifyKind(request.DateDebut, DateTimeKind.Utc),
            DateFin = DateTime.SpecifyKind(request.DateFin, DateTimeKind.Utc),
            IsActive = request.ActiverImmediatement
        };

        _context.AnneeScolaires.Add(annee);
        await _context.SaveChangesAsync(ct);

        // Générer les périodes automatiquement
        var periodes = GenererPeriodes(annee, request.TypePeriode);
        _context.Periodes.AddRange(periodes);
        await _context.SaveChangesAsync(ct);

        return Result<Guid>.Success(annee.Id);
    }

    private static List<Periode> GenererPeriodes(AnneeScolaire annee, TypePeriode typePeriode)
    {
        var periodes = new List<Periode>();
        var duree = annee.DateFin - annee.DateDebut;

        if (typePeriode == TypePeriode.Trimestre)
        {
            var tiers = duree.TotalDays / 3;
            periodes.Add(new Periode
            {
                EcoleId = annee.EcoleId,
                AnneeScolaireId = annee.Id,
                Libelle = "1er Trimestre",
                Type = TypePeriode.Trimestre, Numero = 1,
                DateDebut = annee.DateDebut,
                DateFin = annee.DateDebut.AddDays(tiers)
            });
            periodes.Add(new Periode
            {
                EcoleId = annee.EcoleId,
                AnneeScolaireId = annee.Id,
                Libelle = "2ème Trimestre",
                Type = TypePeriode.Trimestre, Numero = 2,
                DateDebut = annee.DateDebut.AddDays(tiers).AddDays(1),
                DateFin = annee.DateDebut.AddDays(tiers * 2)
            });
            periodes.Add(new Periode
            {
                EcoleId = annee.EcoleId,
                AnneeScolaireId = annee.Id,
                Libelle = "3ème Trimestre",
                Type = TypePeriode.Trimestre, Numero = 3,
                DateDebut = annee.DateDebut.AddDays(tiers * 2).AddDays(1),
                DateFin = annee.DateFin
            });
        }
        else // Semestre
        {
            var moitie = duree.TotalDays / 2;
            periodes.Add(new Periode
            {
                EcoleId = annee.EcoleId,
                AnneeScolaireId = annee.Id,
                Libelle = "1er Semestre",
                Type = TypePeriode.Semestre, Numero = 1,
                DateDebut = annee.DateDebut,
                DateFin = annee.DateDebut.AddDays(moitie)
            });
            periodes.Add(new Periode
            {
                EcoleId = annee.EcoleId,
                AnneeScolaireId = annee.Id,
                Libelle = "2ème Semestre",
                Type = TypePeriode.Semestre, Numero = 2,
                DateDebut = annee.DateDebut.AddDays(moitie).AddDays(1),
                DateFin = annee.DateFin
            });
        }

        return periodes;
    }
}
