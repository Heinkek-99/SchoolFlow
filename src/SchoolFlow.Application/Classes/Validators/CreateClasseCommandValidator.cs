using FluentValidation;
using SchoolFlow.Application.Classes.Commands;

namespace SchoolFlow.Application.Classes.Validators;

public class CreateClasseCommandValidator : AbstractValidator<CreateClasseCommand>
{
    public CreateClasseCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Le code de la classe est obligatoire.")
            .MaximumLength(20).WithMessage("Le code ne peut pas dépasser 20 caractères.");

        RuleFor(x => x.Nom)
            .NotEmpty().WithMessage("Le nom de la classe est obligatoire.")
            .MaximumLength(100).WithMessage("Le nom ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.CapaciteMax)
            .InclusiveBetween(1, 200).WithMessage("La capacité doit être comprise entre 1 et 200 élèves.");

        RuleFor(x => x.AnneeScolaireId)
            .NotEmpty().WithMessage("L'année scolaire est obligatoire.");
    }
}