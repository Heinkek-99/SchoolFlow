using MediatR;
using SchoolFlow.Application.Common.Models;

namespace SchoolFlow.Application.TypesFrais.Commands;

public record ArchiveTypeFraisCommand(Guid Id) : IRequest<Result<string>>;
