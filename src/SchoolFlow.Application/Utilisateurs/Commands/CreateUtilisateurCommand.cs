using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Utilisateurs.Commands;

public record CreateUtilisateurCommand(
    string Username,
    string Password,
    string Nom,
    string Prenom,
    Role Role,
    string? Email = null,
    string? Telephone = null
) : IRequest<Result<Guid>>;
