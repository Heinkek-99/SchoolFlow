using SchoolFlow.Application.Common.Interfaces;

namespace SchoolFlow.Application.Common.Services;
public class CurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; private set; }
    public Guid EcoleId { get; private set; }
    public string? Username { get; private set; }
    public string? Role { get; private set; }
    public string? NomComplet { get; private set; }
    public bool IsAuthenticated => UserId.HasValue;
    public bool IsSuperAdmin => string.Equals(Role, "SuperAdmin", StringComparison.OrdinalIgnoreCase);

    public void SetUser(Guid userId, Guid ecoleId, string username, string role, string nomComplet)
    {
        UserId = userId;
        EcoleId = ecoleId;
        Username = username;
        Role = role;
        NomComplet = nomComplet;
    }

    public void ClearUser()
    {
        UserId = null;
        EcoleId = Guid.Empty;
        Username = null;
        Role = null;
        NomComplet = null;
    }
}