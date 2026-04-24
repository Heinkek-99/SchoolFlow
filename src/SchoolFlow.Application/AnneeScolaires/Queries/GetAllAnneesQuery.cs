using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.AnneeScolaires.Queries;

public record GetAllAnneesQuery : IRequest<Result<List<AnneeScolaireDto>>>;
