namespace SchoolFlow.Application.Common.Interfaces;
public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid EcoleId { get; }  // Guid.Empty pour SuperAdmin
    string? Username { get; }
    string? Role { get; }
    string? NomComplet { get; }
    bool IsAuthenticated { get; }
    bool IsSuperAdmin { get; }

    void SetUser(Guid userId, Guid ecoleId, string username, string role, string nomComplet);
    void ClearUser();
}