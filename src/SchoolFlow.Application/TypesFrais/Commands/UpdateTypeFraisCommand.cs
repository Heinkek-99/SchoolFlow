using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.TypesFrais.Commands;

public record UpdateTypeFraisCommand(
    Guid Id,
    string Libelle,
    string? Description,
    bool IsObligatoire,
    bool GenerationAutomatique,
    Dictionary<Niveau, decimal> MontantsParNiveau
) : IRequest<Result<string>>;
