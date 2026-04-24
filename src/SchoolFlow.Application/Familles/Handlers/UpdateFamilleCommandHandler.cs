using MediatR;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Familles.Commands;

namespace SchoolFlow.Application.Familles.Handlers;

public class UpdateFamilleCommandHandler : IRequestHandler<UpdateFamilleCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public UpdateFamilleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateFamilleCommand request, CancellationToken ct)
    {
        var famille = await _context.Familles.FindAsync(new object[] { request.Id }, ct);

        if (famille == null)
            return Result<bool>.Failure("Famille introuvable");

        famille.MettreAJour(
            nomPere: request.NomPere ?? famille.NomPere,
            prenomPere: request.PrenomPere ?? famille.PrenomPere,
            telephonePere: request.TelephonePere ?? famille.TelephonePere,
            emailPere: request.EmailPere ?? famille.EmailPere,
            nomMere: request.NomMere ?? famille.NomMere,
            prenomMere: request.PrenomMere ?? famille.PrenomMere,
            telephoneMere: request.TelephoneMere ?? famille.TelephoneMere,
            adresse: request.Adresse ?? famille.Adresse,
            ville: request.Ville ?? famille.Ville,
            quartier: famille.QuartierCommune,
            telephonePrincipal: request.TelephonePrincipal ?? famille.TelephonePrincipal
        );

        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
