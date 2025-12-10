using MediatR;
using SchoolFlow.Application.Common.Models;

namespace SchoolFlow.Application.Familles.Commands;

public record UpdateFamilleCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
    public string? TelephonePere { get; init; }
    public string? EmailPere { get; init; }
    public string? TelephoneMere { get; init; }
    public string? EmailMere { get; init; }
    public string? Adresse { get; init; }
    public string? TelephonePrincipal { get; init; }
}
