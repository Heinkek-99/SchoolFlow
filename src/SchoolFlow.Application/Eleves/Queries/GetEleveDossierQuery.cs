using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Eleves.Queries;

public record GetEleveDossierQuery(Guid Id) : IRequest<Result<EleveDossierDto>>;

