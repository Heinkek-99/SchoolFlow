using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.TypesFrais.Commands;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.TypesFrais.Handlers;

public class CreateTypeFraisCommandHandler : IRequestHandler<CreateTypeFraisCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateTypeFraisCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateTypeFraisCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        if (ecoleId == Guid.Empty)
            return Result<Guid>.Failure("Contexte école manquant — reconnectez-vous.");

        var codeExiste = await _context.TypeFrais
            .AnyAsync(t => t.EcoleId == ecoleId && t.Code == request.Code.ToUpper(), ct);

        if (codeExiste)
            return Result<Guid>.Failure($"Un type de frais avec le code '{request.Code}' existe déjà.");

        var typeFrais = new TypeFrais
        {
            EcoleId = ecoleId,
            Code = request.Code.ToUpper().Trim(),
            Libelle = request.Libelle.Trim(),
            Description = request.Description?.Trim(),
            Categorie = request.Categorie,
            IsRecurrent = request.IsRecurrent,
            IsObligatoire = request.IsObligatoire,
            GenerationAutomatique = request.GenerationAutomatique,
            MontantsParNiveau = request.MontantsParNiveau
        };

        _context.TypeFrais.Add(typeFrais);
        await _context.SaveChangesAsync(ct);

        return Result<Guid>.Success(typeFrais.Id);
    }
}
