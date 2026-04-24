using MediatR;
using SchoolFlow.Application.Common.Models;

namespace SchoolFlow.Application.Classes.Commands;

public record ArchiveClasseCommand(Guid Id) : IRequest<Result<string>>;