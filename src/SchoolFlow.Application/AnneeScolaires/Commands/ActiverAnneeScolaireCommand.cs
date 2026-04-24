using MediatR;
using SchoolFlow.Application.Common.Models;

namespace SchoolFlow.Application.AnneeScolaires.Commands;

public record ActiverAnneeScolaireCommand(Guid Id) : IRequest<Result<string>>;
