using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Ecoles.Queries;

public record GetEcoleByIdQuery(Guid EcoleId)
    : IRequest<Result<EcoleDetailDto>>;

public record GetEcoleByCodeQuery(string CodeEcole)
    : IRequest<Result<EcoleDetailDto>>;

public record GetAllEcolesQuery(
    string? Search = null,
    string? Statut = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedResultDto<EcoleListItemDto>>>;