using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Eleves.Commands;

public record UpdateEleveCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
    public string? Nom { get; init; }
    public string? Prenom { get; init; }
    public DateTime? DateNaissance { get; init; }
    public string? LieuNaissance { get; init; }
    public Sexe? Sexe { get; init; }
    public Guid? ClasseId { get; init; }
    public string? PhotoPath { get; init; }
    public string? Nationalite { get; init; }
    public string? GroupeSanguin { get; init; }
    public string? Allergies { get; init; }
    public string? ContactUrgence { get; init; }
    
}