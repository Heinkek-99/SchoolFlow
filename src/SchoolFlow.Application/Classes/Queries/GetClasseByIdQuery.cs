using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Classes.Queries;

public record GetClasseByIdQuery(Guid Id) : IRequest<Result<ClasseDetailDto>>;
