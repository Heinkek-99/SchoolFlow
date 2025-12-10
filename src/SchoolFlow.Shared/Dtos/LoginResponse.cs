namespace SchoolFlow.Shared.Dtos;
public record LoginResponse(
    Guid UserId,
    string Username,
    string Nom,
    string Prenom,
    string Role,
    string Token
);