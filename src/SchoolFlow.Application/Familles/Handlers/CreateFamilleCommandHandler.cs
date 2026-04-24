using MediatR;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Familles.Commands;
using SchoolFlow.Domain.Entities;

public class CreateFamilleCommandHandler : IRequestHandler<CreateFamilleCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateFamilleCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateFamilleCommand request, CancellationToken ct)
    {
        // ← Factory method : lève FamilleCreeeEvent, valide les gardes métier
        var famille = Famille.Creer(
            ecoleId: _currentUser.EcoleId,   // ← MULTI-TENANT
            nomPere: request.NomPere,
            prenomPere: request.PrenomPere,
            telephonePere: request.TelephonePere,
            emailPere: request.EmailPere,
            professionPere: request.ProfessionPere,
            nomMere: request.NomMere,
            prenomMere: request.PrenomMere,
            telephoneMere: request.TelephoneMere,
            adresse: request.Adresse,
            ville: request.Ville,
            quartierCommune: request.QuartierCommune,
            telephonePrincipal: request.TelephonePrincipal,
            telephoneSecondaire: request.TelephoneSecondaire
        );

        _context.Familles.Add(famille);
        await _context.SaveChangesAsync(ct);  // → FamilleCreeeEvent publié ici

        return Result<Guid>.Success(famille.Id);
    }
}