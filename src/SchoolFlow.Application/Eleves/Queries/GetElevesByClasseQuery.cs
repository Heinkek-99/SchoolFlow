namespace SchoolFlow.Application.Eleves.Queries;

using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

public record GetElevesByClasseQuery(Guid ClasseId) : IRequest<Result<List<EleveSimpleDto>>>;

