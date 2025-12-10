using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Eleves.Queries;

public record GetAllElevesQuery(Guid? ClasseId = null, int Page = 1, int PageSize = 50) 
    : IRequest<Result<List<EleveDto>>>;