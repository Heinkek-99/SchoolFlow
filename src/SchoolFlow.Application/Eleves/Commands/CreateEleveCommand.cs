using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Eleves.Commands;

public record CreateEleveCommand : IRequest<Result<CreateEleveResponse>>
{
    public string Nom { get; init; } = string.Empty;
    public string Prenom { get; init; } = string.Empty;
    public DateTime DateNaissance { get; init; }
    public string LieuNaissance { get; init; } = string.Empty;
    public Sexe Sexe { get; init; }
    public Guid FamilleId { get; init; }
    public Guid ClasseId { get; init; }
    public string? PhotoPath { get; init; }
}

public record CreateEleveResponse(Guid EleveId, string Matricule);

