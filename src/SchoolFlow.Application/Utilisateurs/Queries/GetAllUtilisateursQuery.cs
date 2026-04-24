using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Utilisateurs.Queries;

public record GetAllUtilisateursQuery(string? Search = null) : IRequest<Result<List<UtilisateurDto>>>;
