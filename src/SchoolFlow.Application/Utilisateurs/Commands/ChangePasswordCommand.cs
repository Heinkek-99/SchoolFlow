using MediatR;
using SchoolFlow.Application.Common.Models;

namespace SchoolFlow.Application.Utilisateurs.Commands;

public record ChangePasswordCommand(
    Guid UtilisateurId,
    string AncienMotDePasse,
    string NouveauMotDePasse
) : IRequest<Result<string>>;
