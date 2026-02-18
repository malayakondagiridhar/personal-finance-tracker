using System.Security.Claims;

namespace PersonalFinanceTracker.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetClaimedUserIdOrNull(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? principal.FindFirstValue("user_id")
                  ?? principal.FindFirstValue("sub");

        return Guid.TryParse(raw, out var userId) ? userId : null;
    }

    public static string GetExternalAuthId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("sub")
               ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new UnauthorizedAccessException("Authenticated token must include a subject claim.");
    }
}
