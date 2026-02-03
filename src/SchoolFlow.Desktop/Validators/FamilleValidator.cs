using FluentValidation;

namespace SchoolFlow.Desktop.Validators;

/// <summary>
/// Validateur pour la création/modification d'une famille.
/// </summary>
public class FamilleValidator : AbstractValidator<FamilleFormModel>
{
    public FamilleValidator()
    {
        RuleFor(x => x.NomPere)
            .NotEmpty().WithMessage("Le nom du père est obligatoire.")
            .MaximumLength(100).WithMessage("Le nom du père ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.PrenomPere)
            .MaximumLength(100).WithMessage("Le prénom du père ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.NomMere)
            .MaximumLength(100).WithMessage("Le nom de la mère ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.PrenomMere)
            .MaximumLength(100).WithMessage("Le prénom de la mère ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.Telephone)
            .NotEmpty().WithMessage("Le numéro de téléphone est obligatoire.")
            .Matches(@"^[0-9+\s\-]{8,20}$").WithMessage("Le format du téléphone est invalide.");

        RuleFor(x => x.TelephoneSecondaire)
            .Matches(@"^[0-9+\s\-]{8,20}$").WithMessage("Le format du téléphone secondaire est invalide.")
            .When(x => !string.IsNullOrEmpty(x.TelephoneSecondaire));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("L'adresse email n'est pas valide.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Adresse)
            .MaximumLength(250).WithMessage("L'adresse ne peut pas dépasser 250 caractères.");
    }
}

/// <summary>
/// Modèle de formulaire pour une famille.
/// </summary>
public class FamilleFormModel
{
    public Guid? Id { get; set; }
    public string NomPere { get; set; } = string.Empty;
    public string? PrenomPere { get; set; }
    public string? NomMere { get; set; }
    public string? PrenomMere { get; set; }
    public string Telephone { get; set; } = string.Empty;
    public string? TelephoneSecondaire { get; set; }
    public string? Email { get; set; }
    public string? Adresse { get; set; }
    public string? Quartier { get; set; }
    public string? Ville { get; set; }

    public bool IsNew => !Id.HasValue;
}
