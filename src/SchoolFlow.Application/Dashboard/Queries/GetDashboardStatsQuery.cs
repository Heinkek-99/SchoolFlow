using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Dashboard.Queries;

public record GetDashboardStatsQuery(Guid UtilisateurId, string Role) : IRequest<Result<DashboardStatsDto>>;




