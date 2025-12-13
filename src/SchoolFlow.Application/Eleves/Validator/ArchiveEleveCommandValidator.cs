using FluentValidation;
using SchoolFlow.Application.Eleves.Commands;

namespace SchoolFlow.Application.Eleves.Validator;

public class ArchiveEleveCommandValidator : AbstractValidator<ArchiveEleveCommand>
{
    public ArchiveEleveCommandValidator()
    {
        RuleFor(x => x.EleveId).NotEmpty();
        RuleFor(x => x.MotifArchivage)
            .NotEmpty().WithMessage("Le motif d'archivage est obligatoire")
            .MaximumLength(200);
        RuleFor(x => x.ArchivedBy).NotEmpty();
    }
}