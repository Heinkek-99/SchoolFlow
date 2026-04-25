using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Classes.Commands;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Classes.Handlers;

public class CreateClasseCommandHandler : IRequestHandler<CreateClasseCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateClasseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateClasseCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        if (ecoleId == Guid.Empty)
            return Result<Guid>.Failure("Contexte école manquant — reconnectez-vous.");

        var codeExiste = await _context.Classes
            .AnyAsync(c => c.EcoleId == ecoleId
                        && c.AnneeScolaireId == request.AnneeScolaireId
                        && c.Code.ToLower() == request.Code.ToLower().Trim(), ct);

        if (codeExiste)
            return Result<Guid>.Failure($"Une classe avec le code '{request.Code}' existe déjà pour cette année scolaire.");

        var anneeExiste = await _context.AnneeScolaires
            .AnyAsync(a => a.Id == request.AnneeScolaireId && a.EcoleId == ecoleId, ct);

        if (!anneeExiste)
            return Result<Guid>.Failure("Année scolaire introuvable.");

        var classe = Classe.Creer(
            request.Code, request.Nom, request.Niveau,
            request.SousSysteme, request.Section, request.CapaciteMax,
            request.AnneeScolaireId, ecoleId, request.TitulaireId);

        _context.Classes.Add(classe);
        await _context.SaveChangesAsync(ct);

        return Result<Guid>.Success(classe.Id);
    }
}