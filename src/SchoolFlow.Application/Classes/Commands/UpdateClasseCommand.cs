using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Classes.Commands;

public record UpdateClasseCommand(
    Guid Id,
    string Nom,
    Niveau Niveau,
    SousSysteme SousSysteme,
    string? Section,
    int CapaciteMax,
    Guid? TitulaireId
) : IRequest<Result<string>>;