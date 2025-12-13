using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;
namespace SchoolFlow.Application.Paiements.Queries;

public record GetAllPaiementsQuery(
    DateTime? DateDebut = null,
    DateTime? DateFin = null,
    string? ModePaiement = null,
    Guid? FamilleId = null,
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<Result<PagedList<PaiementListItemDto>>>;
