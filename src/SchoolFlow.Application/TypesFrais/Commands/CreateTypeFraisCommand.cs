using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.TypesFrais.Commands;

public record CreateTypeFraisCommand(
    string Code,
    string Libelle,
    string? Description,
    CategorieFrais Categorie,
    bool IsRecurrent,
    bool IsObligatoire,
    bool GenerationAutomatique,
    Dictionary<Niveau, decimal> MontantsParNiveau
) : IRequest<Result<Guid>>;
