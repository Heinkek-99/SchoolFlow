using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.TypesFrais.Queries;

public record GetAllTypesFraisQuery : IRequest<Result<List<TypeFraisDto>>>;
