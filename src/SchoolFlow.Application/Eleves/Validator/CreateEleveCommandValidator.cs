namespace SchoolFlow.Application.Eleves.Validator;
using System;
using FluentValidation;
using MediatR;
using SchoolFlow.Application.Eleves.Commands;

public class CreateEleveCommandValidator : AbstractValidator<CreateEleveCommand>
{
    public CreateEleveCommandValidator()
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Prenom).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateNaissance)
            .NotEmpty()
            .LessThan(DateTime.Today.AddYears(-3)).WithMessage("L'élève doit avoir au moins 3 ans")
            .GreaterThan(DateTime.Today.AddYears(-25)).WithMessage("L'élève doit avoir moins de 25 ans");
        RuleFor(x => x.LieuNaissance).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FamilleId).NotEmpty();
        RuleFor(x => x.ClasseId).NotEmpty();
    }
}