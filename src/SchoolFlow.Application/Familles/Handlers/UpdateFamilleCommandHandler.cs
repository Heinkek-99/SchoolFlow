using MediatR;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Familles.Commands;

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

        // Père
        if (!string.IsNullOrWhiteSpace(request.NomPere))
            famille.NomPere = request.NomPere;
        if (request.PrenomPere != null)
            famille.PrenomPere = request.PrenomPere;
        if (!string.IsNullOrWhiteSpace(request.TelephonePere))
            famille.TelephonePere = request.TelephonePere;
        if (!string.IsNullOrWhiteSpace(request.EmailPere))
            famille.EmailPere = request.EmailPere;

        // Mère
        if (request.NomMere != null)
            famille.NomMere = request.NomMere;
        if (request.PrenomMere != null)
            famille.PrenomMere = request.PrenomMere;
        if (!string.IsNullOrWhiteSpace(request.TelephoneMere))
            famille.TelephoneMere = request.TelephoneMere;
        if (!string.IsNullOrWhiteSpace(request.EmailMere))
            famille.EmailMere = request.EmailMere;

        // Contact principal
        if (!string.IsNullOrWhiteSpace(request.TelephonePrincipal))
            famille.TelephonePrincipal = request.TelephonePrincipal;

        // Adresse
        if (request.Adresse != null)
            famille.Adresse = request.Adresse;
        if (request.Ville != null)
            famille.Ville = request.Ville;

        famille.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}