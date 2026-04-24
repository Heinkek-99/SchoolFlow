using MediatR;
using SchoolFlow.Application.Common.Models;

namespace SchoolFlow.Application.Utilisateurs.Commands;

public record UpdateUtilisateurCommand(
    Guid Id,
    string Nom,
    string Prenom,
    string? Email,
    string? Telephone
) : IRequest<Result<string>>;
