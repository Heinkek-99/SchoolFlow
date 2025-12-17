using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;
namespace SchoolFlow.Application.Dashboard.Queries;

public record GetFamillesImpayesQuery(
    int? LimiteResultats = null,
    int? JoursRetardMinimum = null
) : IRequest<Result<List<FamilleImpayeDto>>>;
