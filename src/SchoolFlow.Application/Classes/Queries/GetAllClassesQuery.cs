using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Classes.Queries;

public record GetAllClassesQuery : IRequest<Result<List<ClasseDto>>>;
