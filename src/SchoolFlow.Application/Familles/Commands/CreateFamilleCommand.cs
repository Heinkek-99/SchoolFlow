using MediatR;
using SchoolFlow.Application.Common.Models;


namespace SchoolFlow.Application.Familles.Commands;

public record CreateFamilleCommand : IRequest<Result<Guid>>
{
    public string NomPere { get; init; } = string.Empty;
    public string? PrenomPere { get; init; }
    public string? TelephonePere { get; init; }
    public string? EmailPere { get; init; }
    public string? ProfessionPere { get; init; }
    
    public string? NomMere { get; init; }
    public string? PrenomMere { get; init; }
    public string? TelephoneMere { get; init; }
    public string? EmailMere { get; init; }
    public string? ProfessionMere { get; init; }
    
    public string Adresse { get; init; } = string.Empty;
    public string Ville { get; init; } = string.Empty;
    public string TelephonePrincipal { get; init; } = string.Empty;
    public string? TelephoneSecondaire { get; init; }
    public string? QuartierCommune { get; init; }
}
