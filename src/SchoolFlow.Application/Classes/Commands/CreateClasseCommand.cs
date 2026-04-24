using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Classes.Commands;

public record CreateClasseCommand(
    string Code,
    string Nom,
    Niveau Niveau,
    SousSysteme SousSysteme,
    string? Section,
    int CapaciteMax,
    Guid AnneeScolaireId,
    Guid? TitulaireId
) : IRequest<Result<Guid>>;