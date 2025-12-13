using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Paiements.Queries;

public record ProposerVentilationQuery(Guid FamilleId, decimal MontantTotal) : IRequest<Result<List<VentilationProposeeDto>>>;