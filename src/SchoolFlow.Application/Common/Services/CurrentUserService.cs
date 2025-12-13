using SchoolFlow.Application.Common.Interfaces;

namespace SchoolFlow.Application.Common.Services;
public class CurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; private set; }
    public string? Username { get; private set; }
    public string? Role { get; private set; }
    public string? NomComplet { get; private set; }
    public bool IsAuthenticated => UserId.HasValue;

    public void SetUser(Guid userId, string username, string role, string nomComplet)
    {
        UserId = userId;
        Username = username;
        Role = role;
        NomComplet = nomComplet;
    }

    public void ClearUser()
    {
        UserId = null;
        Username = null;
        Role = null;
        NomComplet = null;
    }
}