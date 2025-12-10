using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Familles.Queries;

public record GetFamilleByIdQuery(Guid Id) : IRequest<Result<FamilleDetailDto>>;



