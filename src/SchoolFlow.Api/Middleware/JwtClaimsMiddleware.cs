namespace SchoolFlow.Api.Middleware;

using System.IdentityModel.Tokens.Jwt;
using SchoolFlow.Application.Common.Interfaces;

public class JwtClaimsMiddleware
{
    private readonly RequestDelegate _next;

    public JwtClaimsMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUserService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId   = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var ecoleIdStr = context.User.FindFirst("ecoleId")?.Value;  // fixed: was "ecole_id"
            var username = context.User.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;
            var role     = context.User.FindFirst("role")?.Value;
            var prenom   = context.User.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value;
            var nom      = context.User.FindFirst(JwtRegisteredClaimNames.FamilyName)?.Value;

            if (Guid.TryParse(userId, out var userGuid) && username is not null && role is not null)
            {
                // SuperAdmin a ecoleId vide → Guid.Empty (sentinel)
                var ecoleGuid = Guid.TryParse(ecoleIdStr, out var parsed) ? parsed : Guid.Empty;

                currentUserService.SetUser(
                    userId: userGuid,
                    ecoleId: ecoleGuid,
                    username: username,
                    role: role,
                    nomComplet: $"{prenom} {nom}".Trim()
                );
            }
        }

        await _next(context);
    }
}
 