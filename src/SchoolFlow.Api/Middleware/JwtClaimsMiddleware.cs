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
            var userId     = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                          ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var ecoleIdStr = context.User.FindFirst("ecoleId")?.Value;
            var username   = context.User.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value
                          ?? context.User.FindFirst("unique_name")?.Value
                          ?? context.User.Identity.Name;
            var role       = context.User.FindFirst("role")?.Value
                          ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var prenom     = context.User.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value ?? "";
            var nom        = context.User.FindFirst(JwtRegisteredClaimNames.FamilyName)?.Value ?? "";

            // SuperAdmin → ecoleIdStr = "" → Guid.Empty (sentinel, pas d'exception)
            var ecoleId = Guid.TryParse(ecoleIdStr, out var parsedEcole) ? parsedEcole : Guid.Empty;

            // SetUser toujours appelé si userId parseable — username/role default à "" si remappés
            if (Guid.TryParse(userId, out var userGuid))
            {
                currentUserService.SetUser(
                    userId: userGuid,
                    ecoleId: ecoleId,
                    username: username ?? "",
                    role: role ?? "",
                    nomComplet: $"{prenom} {nom}".Trim()
                );
            }
        }

        await _next(context);
    }
}
