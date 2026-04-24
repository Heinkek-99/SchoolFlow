using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Utilisateurs.Queries;

public record GetUtilisateurByIdQuery(Guid Id) : IRequest<Result<UtilisateurDto>>;
