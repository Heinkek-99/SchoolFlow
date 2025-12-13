using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;
namespace SchoolFlow.Application.Dashboard.Queries;

public record GetStatsFinancieresQuery(
    DateTime? DateDebut = null,
    DateTime? DateFin = null
) : IRequest<Result<StatsFinancieresDetailDto>>;