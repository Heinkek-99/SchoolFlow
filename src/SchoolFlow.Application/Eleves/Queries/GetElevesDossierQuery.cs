using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Eleves.Queries;

public record GetElevesDossierQuery(Guid Id) : IRequest<Result<EleveDossierDto>>;

