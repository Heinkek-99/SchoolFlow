using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;


namespace SchoolFlow.Application.Paiements.Queries;

public record GetPaiementByNumeroQuery(string Numero) : IRequest<Result<PaiementDetailDto>>;
