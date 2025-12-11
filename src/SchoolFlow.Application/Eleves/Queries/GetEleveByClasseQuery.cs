namespace SchoolFlow.Application.Eleves.Queries;

using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

public record GetEleveByClasseQuery(Guid ClasseId) : IRequest<Result<List<EleveDto>>>;

public record GetEleveByClasseQueryWithStatutQuery(
    Guid ClasseId,
    string Statut
) : IRequest<Result<List<EleveDto>>>;


public record GetEleveByClasseQueryWithSoldeQuery(
    Guid ClasseId,
    decimal MinimumSolde
) : IRequest<Result<List<EleveDto>>>;

public record GetEleveByClasseQueryWithStatutAndSoldeQuery(
    Guid ClasseId,
    string Statut,
    decimal MinimumSolde
) : IRequest<Result<List<EleveDto>>>;

public record GetEleveByClasseQueryWithoutFiltersQuery(
    Guid ClasseId
) : IRequest<Result<List<EleveDto>>>;

public record GetEleveByClasseQueryWithAllFiltersQuery(
    Guid ClasseId,
    string Statut,
    decimal MinimumSolde
) : IRequest<Result<List<EleveDto>>>;
