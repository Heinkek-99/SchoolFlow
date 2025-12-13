using System.Collections.Generic;
using MediatR;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;   

namespace SchoolFlow.Application.Paiements.Queries;
public record GetHistoriquePaiementsFamilleQuery(Guid FamilleId) : IRequest<Result<List<PaiementListItemDto>>>;