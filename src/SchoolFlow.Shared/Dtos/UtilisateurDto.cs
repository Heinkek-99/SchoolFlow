namespace SchoolFlow.Shared.Dtos;

public record UtilisateurDto(
    Guid Id,
    string Username,
    string Nom,
    string Prenom,
    string NomComplet,
    string? Email,
    string? Telephone,
    string Role,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime CreatedAt
);
