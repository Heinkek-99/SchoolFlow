// ============================================================
// FICHIER : src/SchoolFlow.Application/Ecoles/Validators/CreerEcoleCommandValidator.cs
// ACTION  : Fichier à CRÉER
// ============================================================

using FluentValidation;
using SchoolFlow.Application.Ecoles.Commands;

namespace SchoolFlow.Application.Ecoles.Validators;

public class CreerEcoleCommandValidator : AbstractValidator<CreerEcoleCommand>
{
    public CreerEcoleCommandValidator()
    {
        // ── ÉCOLE ────────────────────────────────────────────────────────────
        RuleFor(x => x.NomEcole)
            .NotEmpty().WithMessage("Le nom de l'école est obligatoire.")
            .MinimumLength(3).WithMessage("Le nom doit faire au moins 3 caractères.")
            .MaximumLength(200).WithMessage("Le nom ne peut dépasser 200 caractères.");

        RuleFor(x => x.TypeEtablissement)
            .IsInEnum().WithMessage("Type d'établissement invalide.");

        RuleFor(x => x.Adresse)
            .NotEmpty().WithMessage("L'adresse est obligatoire.")
            .MaximumLength(500);

        RuleFor(x => x.Ville)
            .NotEmpty().WithMessage("La ville est obligatoire.")
            .MaximumLength(100);

        RuleFor(x => x.Pays)
            .NotEmpty().WithMessage("Le pays est obligatoire.")
            .MaximumLength(50);

        RuleFor(x => x.TelephonePrincipal)
            .NotEmpty().WithMessage("Le téléphone principal est obligatoire.")
            .Matches(@"^[+]?[\d\s\-]{8,20}$")
            .WithMessage("Format de téléphone invalide.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Format d'email invalide.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        // ── DIRECTEUR ────────────────────────────────────────────────────────
        RuleFor(x => x.NomDirecteur)
            .NotEmpty().WithMessage("Le nom du directeur est obligatoire.")
            .MaximumLength(100);

        RuleFor(x => x.PrenomDirecteur)
            .NotEmpty().WithMessage("Le prénom du directeur est obligatoire.")
            .MaximumLength(100);

        RuleFor(x => x.EmailDirecteur)
            .EmailAddress().WithMessage("Format d'email directeur invalide.")
            .When(x => !string.IsNullOrEmpty(x.EmailDirecteur));

        // ── COMPTE ADMIN ──────────────────────────────────────────────────────
        RuleFor(x => x.UsernameAdmin)
            .NotEmpty().WithMessage("Le nom d'utilisateur Admin est obligatoire.")
            .MinimumLength(4).WithMessage("Le nom d'utilisateur doit faire au moins 4 caractères.")
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z0-9._-]+$")
            .WithMessage("Caractères autorisés : lettres, chiffres, . _ -");

        RuleFor(x => x.PasswordAdmin)
            .NotEmpty().WithMessage("Le mot de passe est obligatoire.")
            .MinimumLength(8).WithMessage("Le mot de passe doit faire au moins 8 caractères.")
            .Matches(@"[A-Z]").WithMessage("Au moins une majuscule requise.")
            .Matches(@"[0-9]").WithMessage("Au moins un chiffre requis.");
    }
}

// ─── VALIDATOR : Valider École ────────────────────────────────────────────────

public class ValiderEcoleCommandValidator : AbstractValidator<ValiderEcoleCommand>
{
    public ValiderEcoleCommandValidator()
    {
        RuleFor(x => x.EcoleId)
            .NotEmpty().WithMessage("L'identifiant de l'école est obligatoire.");
    }
}

// ─── VALIDATOR : Rejeter École ────────────────────────────────────────────────

public class RejeterEcoleCommandValidator : AbstractValidator<RejeterEcoleCommand>
{
    public RejeterEcoleCommandValidator()
    {
        RuleFor(x => x.EcoleId)
            .NotEmpty().WithMessage("L'identifiant de l'école est obligatoire.");

        RuleFor(x => x.Motif)
            .NotEmpty().WithMessage("Le motif de rejet est obligatoire.")
            .MinimumLength(10).WithMessage("Le motif doit être explicite (min. 10 caractères).")
            .MaximumLength(500);
    }
}