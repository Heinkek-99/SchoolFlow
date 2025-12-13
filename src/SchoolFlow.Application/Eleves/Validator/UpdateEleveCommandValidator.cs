using FluentValidation;
using SchoolFlow.Application.Eleves.Commands;

namespace SchoolFlow.Application.Eleves.Validator;

// ============================================
// UpdateEleveCommandValidator
// ============================================
// Validator pour la commande de mise à jour d'un élève
// ============================================

public class UpdateEleveCommandValidator : AbstractValidator<UpdateEleveCommand>
{
    public UpdateEleveCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        
        When(x => x.Nom != null, () =>
        {
            RuleFor(x => x.Nom).MaximumLength(100);
        });
        
        When(x => x.Prenom != null, () =>
        {
            RuleFor(x => x.Prenom).MaximumLength(100);
        });
        
        When(x => x.DateNaissance != null, () =>
        {
            RuleFor(x => x.DateNaissance!.Value)
                .LessThan(DateTime.Today.AddYears(-3))
                .WithMessage("L'élève doit avoir au moins 3 ans")
                .GreaterThan(DateTime.Today.AddYears(-25))
                .WithMessage("L'élève doit avoir moins de 25 ans");
        });
    }
}