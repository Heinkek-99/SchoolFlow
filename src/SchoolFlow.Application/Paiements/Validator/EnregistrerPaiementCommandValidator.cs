
using FluentValidation;
using SchoolFlow.Application.Paiements.Commands;

namespace SchoolFlow.Application.Paiements.Validator;

public class EnregistrerPaiementCommandValidator : AbstractValidator<EnregistrerPaiementCommand>
{
    public EnregistrerPaiementCommandValidator()
    {
        RuleFor(x => x.FamilleId)
            .NotEmpty()
            .WithMessage("L'identifiant famille est obligatoire");

        RuleFor(x => x.MontantTotal)
            .GreaterThan(0)
            .WithMessage("Le montant total doit être supérieur à zéro")
            .LessThanOrEqualTo(10_000_000)
            .WithMessage("Le montant total semble anormalement élevé");

        RuleFor(x => x.DatePaiement)
            .NotEmpty()
            .WithMessage("La date de paiement est obligatoire")
            .LessThanOrEqualTo(DateTime.Now)
            .WithMessage("La date de paiement ne peut pas être dans le futur")
            .GreaterThan(DateTime.Now.AddYears(-1))
            .WithMessage("La date de paiement ne peut pas dépasser 1 an");

        RuleFor(x => x.Reference)
            .MaximumLength(100)
            .WithMessage("La référence externe ne peut pas dépasser 100 caractères");

        RuleFor(x => x.Commentaire)
            .MaximumLength(500)
            .WithMessage("Les remarques ne peuvent pas dépasser 500 caractères");

        RuleFor(x => x.Ventilations)
            .NotEmpty()
            .WithMessage("Au moins une ventilation est requise")
            .Must(ventilations => ventilations.Count <= 20)
            .WithMessage("Nombre maximum de ventilations: 20");

        RuleFor(x => x)
            .Must(cmd => 
            {
                var totalVentile = cmd.Ventilations.Sum(v => v.Montant);
                return Math.Abs(totalVentile - cmd.MontantTotal) < 0.01m;
            })
            .WithMessage("Le total des ventilations doit égaler le montant total du paiement");

        RuleForEach(x => x.Ventilations).ChildRules(ventilation =>
        {
            ventilation.RuleFor(v => v.EleveId)
                .NotEmpty()
                .WithMessage("L'identifiant élève est obligatoire");

            ventilation.RuleFor(v => v.Montant)
                .GreaterThan(0)
                .WithMessage("Le montant de ventilation doit être supérieur à zéro");

            ventilation.RuleFor(v => v.Remarque)
                .MaximumLength(200)
                .WithMessage("La remarque ne peut pas dépasser 200 caractères");
        });
    }
}

// public class EnregistrerPaiementCommandValidator : AbstractValidator<EnregistrerPaiementCommand>
// {
//     public EnregistrerPaiementCommandValidator()
//     {
//         RuleFor(x => x.MontantTotal)
//             .GreaterThan(0).WithMessage("Le montant doit être supérieur à 0");

//         RuleFor(x => x.DatePaiement)
//             .LessThanOrEqualTo(DateTime.Today).WithMessage("La date ne peut pas être dans le futur");

//         RuleFor(x => x.Ventilations)
//             .NotEmpty().WithMessage("Au moins une ventilation est requise")
//             .Must((cmd, ventilations) => ventilations.Sum(v => v.MontantVentile) == cmd.MontantTotal)
//             .WithMessage("La somme des ventilations doit égaler le montant total");
//     }
// }