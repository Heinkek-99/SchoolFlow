namespace SchoolFlow.Shared.Dtos;
public record LoginResponse(
    Guid UserId,
    Guid? EcoleId,
    string NomEcole,
    string Username,
    string Nom,
    string Prenom,
    string Role,
    string Token
);