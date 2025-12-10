using FluentValidation;
using MediatR;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Familles.Commands;

public record CreateFamilleCommand : IRequest<Result<Guid>>
{
    public string NomPere { get; init; } = string.Empty;
    public string? PrenomPere { get; init; }
    public string? TelephonePere { get; init; }
    public string? EmailPere { get; init; }
    
    public string? NomMere { get; init; }
    public string? PrenomMere { get; init; }
    public string? TelephoneMere { get; init; }
    
    public string Adresse { get; init; } = string.Empty;
    public string Ville { get; init; } = string.Empty;
    public string TelephonePrincipal { get; init; } = string.Empty;
}

public class CreateFamilleCommandValidator : AbstractValidator<CreateFamilleCommand>
{
    public CreateFamilleCommandValidator()
    {
        RuleFor(x => x.NomPere)
            .NotEmpty().WithMessage("Le nom du père est obligatoire")
            .MaximumLength(100);

        RuleFor(x => x.TelephonePrincipal)
            .NotEmpty().WithMessage("Un numéro de téléphone est obligatoire")
            .Matches(@"^\+?[0-9\s\-()]{8,20}$").WithMessage("Format téléphone invalide");

        RuleFor(x => x.Adresse)
            .NotEmpty().WithMessage("L'adresse est obligatoire")
            .MaximumLength(500);

        RuleFor(x => x.Ville)
            .NotEmpty().WithMessage("La ville est obligatoire");
    }
}

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