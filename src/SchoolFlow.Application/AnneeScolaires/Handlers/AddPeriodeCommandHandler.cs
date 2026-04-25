using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.AnneeScolaires.Commands;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.AnneeScolaires.Handlers;

public class AddPeriodeCommandHandler : IRequestHandler<AddPeriodeCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddPeriodeCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(AddPeriodeCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var annee = await _context.AnneeScolaires
            .FirstOrDefaultAsync(a => a.Id == request.AnneeScolaireId && a.EcoleId == ecoleId, ct);

        if (annee is null)
            return Result<Guid>.Failure("Année scolaire introuvable.");

        if (request.DateFin <= request.DateDebut)
            return Result<Guid>.Failure("La date de fin doit être postérieure à la date de début.");

        if (request.DateDebut < annee.DateDebut || request.DateFin > annee.DateFin)
            return Result<Guid>.Failure("Les dates de la période doivent être comprises dans l'année scolaire.");

        var numeroExiste = await _context.Periodes
            .AnyAsync(p => p.AnneeScolaireId == request.AnneeScolaireId
                        && p.Numero == request.Numero, ct);

        if (numeroExiste)
            return Result<Guid>.Failure($"Une période avec le numéro {request.Numero} existe déjà pour cette année.");

        var periode = new Periode
        {
            EcoleId = ecoleId,
            AnneeScolaireId = request.AnneeScolaireId,
            Libelle = request.Libelle.Trim(),
            Type = request.Type,
            Numero = request.Numero,
            DateDebut = DateTime.SpecifyKind(request.DateDebut, DateTimeKind.Utc),
            DateFin = DateTime.SpecifyKind(request.DateFin, DateTimeKind.Utc)
        };

        _context.Periodes.Add(periode);
        await _context.SaveChangesAsync(ct);

        return Result<Guid>.Success(periode.Id);
    }
}
