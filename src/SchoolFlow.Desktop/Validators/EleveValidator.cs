using FluentValidation;

namespace SchoolFlow.Desktop.Validators;

/// <summary>
/// Validateur pour la création/modification d'un élève.
/// </summary>
public class EleveValidator : AbstractValidator<EleveFormModel>
{
    public EleveValidator()
    {
        RuleFor(x => x.Nom)
            .NotEmpty().WithMessage("Le nom est obligatoire.")
            .MaximumLength(100).WithMessage("Le nom ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.Prenom)
            .NotEmpty().WithMessage("Le prénom est obligatoire.")
            .MaximumLength(100).WithMessage("Le prénom ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.DateNaissance)
            .NotEmpty().WithMessage("La date de naissance est obligatoire.")
            .LessThan(DateTime.Today).WithMessage("La date de naissance doit être dans le passé.")
            .GreaterThan(DateTime.Today.AddYears(-25)).WithMessage("L'élève ne peut pas avoir plus de 25 ans.");

        RuleFor(x => x.LieuNaissance)
            .MaximumLength(100).WithMessage("Le lieu de naissance ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.Sexe)
            .NotEmpty().WithMessage("Le sexe est obligatoire.")
            .Must(x => x == "M" || x == "F").WithMessage("Le sexe doit être 'M' ou 'F'.");

        RuleFor(x => x.FamilleId)
            .NotEmpty().WithMessage("La famille est obligatoire.");

        RuleFor(x => x.ClasseId)
            .NotEmpty().WithMessage("La classe est obligatoire.");

        RuleFor(x => x.Nationalite)
            .MaximumLength(50).WithMessage("La nationalité ne peut pas dépasser 50 caractères.");

        RuleFor(x => x.Allergies)
            .MaximumLength(500).WithMessage("Les allergies ne peuvent pas dépasser 500 caractères.");

        RuleFor(x => x.Observations)
            .MaximumLength(1000).WithMessage("Les observations ne peuvent pas dépasser 1000 caractères.");
    }
}

/// <summary>
/// Modèle de formulaire pour un élève.
/// </summary>
public class EleveFormModel
{
    public Guid? Id { get; set; }
    public string? Matricule { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public DateTime? DateNaissance { get; set; }
    public string? LieuNaissance { get; set; }
    public string Sexe { get; set; } = "M";
    public Guid FamilleId { get; set; }
    public Guid ClasseId { get; set; }
    public string? Nationalite { get; set; } = "Camerounaise";
    public string? Photo { get; set; }
    public string? Allergies { get; set; }
    public string? Observations { get; set; }
    public bool EstBoursier { get; set; }
    public bool EstRedoublant { get; set; }

    public bool IsNew => !Id.HasValue;
}
