namespace SchoolFlow.Application.Common.Interfaces;
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Username { get; }
    string? Role { get; }
    string? NomComplet { get; }
    bool IsAuthenticated { get; }
    
    void SetUser(Guid userId, string username, string role, string nomComplet);
    void ClearUser();
}