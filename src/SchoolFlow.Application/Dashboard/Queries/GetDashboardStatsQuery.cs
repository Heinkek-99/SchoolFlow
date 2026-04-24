using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Dashboard.Queries;

public record GetDashboardStatsQuery() : IRequest<Result<DashboardStats>>;




