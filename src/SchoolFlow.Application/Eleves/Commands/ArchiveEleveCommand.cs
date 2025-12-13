using MediatR;
using SchoolFlow.Application.Common.Models;

namespace SchoolFlow.Application.Eleves.Commands;

public record ArchiveEleveCommand : IRequest<Result<bool>>
{
    public Guid EleveId { get; init; }
    public string MotifArchivage { get; init; } = string.Empty;
    public string? Commentaire { get; init; }
    public Guid ArchivedBy { get; init; }
}