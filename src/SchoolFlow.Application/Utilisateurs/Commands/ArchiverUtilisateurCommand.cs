using MediatR;
using SchoolFlow.Application.Common.Models;

namespace SchoolFlow.Application.Utilisateurs.Commands;

public record ArchiverUtilisateurCommand(Guid Id) : IRequest<Result<string>>;
