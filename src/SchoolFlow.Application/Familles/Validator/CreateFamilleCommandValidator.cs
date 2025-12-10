using FluentValidation;
using SchoolFlow.Application.Familles.Commands;

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