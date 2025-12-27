
using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Eleves.Queries;

public record GetElevesByFamilleQuery(Guid FamilleId) : IRequest<Result<List<EleveSimpleDto>>>;
