using MediatR;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Familles.Commands;
using SchoolFlow.Domain.Entities;

public class CreateFamilleCommandHandler : IRequestHandler<CreateFamilleCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateFamilleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateFamilleCommand request, CancellationToken ct)
    {
        var famille = new Famille
        {
            NomPere = request.NomPere,
            PrenomPere = request.PrenomPere,
            TelephonePere = request.TelephonePere,
            EmailPere = request.EmailPere,
            NomMere = request.NomMere,
            PrenomMere = request.PrenomMere,
            TelephoneMere = request.TelephoneMere,
            Adresse = request.Adresse,
            Ville = request.Ville,
            TelephonePrincipal = request.TelephonePrincipal
        };

        _context.Familles.Add(famille);
        await _context.SaveChangesAsync(ct);

        return Result<Guid>.Success(famille.Id);
    }
}