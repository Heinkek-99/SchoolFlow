using MediatR;
using SchoolFlow.Application.Common.Models;

namespace SchoolFlow.Application.Familles.Commands;

public record UpdateFamilleCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }

    // Père
    public string? NomPere { get; init; }
    public string? PrenomPere { get; init; }
    public string? TelephonePere { get; init; }
    public string? EmailPere { get; init; }

    // Mère
    public string? NomMere { get; init; }
    public string? PrenomMere { get; init; }
    public string? TelephoneMere { get; init; }
    public string? EmailMere { get; init; }

    // Contact principal
    public string? TelephonePrincipal { get; init; }

    // Adresse
    public string? Adresse { get; init; }
    public string? Ville { get; init; }
}