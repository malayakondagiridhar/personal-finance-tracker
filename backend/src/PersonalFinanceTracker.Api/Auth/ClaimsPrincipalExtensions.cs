using System.Security.Claims;

namespace PersonalFinanceTracker.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetRequiredUserId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? principal.FindFirstValue("user_id")
                  ?? principal.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(raw) || !Guid.TryParse(raw, out var userId))
        {
            throw new UnauthorizedAccessException("Authenticated token must include a valid user identifier claim.");
        }

        return userId;
    }

    public static string GetExternalAuthId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("sub")
               ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new UnauthorizedAccessException("Authenticated token must include a subject claim.");
    }
}
