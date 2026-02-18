namespace PersonalFinanceTracker.Api.Auth;

public static class HttpContextUserExtensions
{
    public const string InternalUserIdItemKey = "InternalUserId";

    public static Guid GetRequiredUserId(this HttpContext httpContext)
    {
        if (httpContext.Items.TryGetValue(InternalUserIdItemKey, out var raw)
            && raw is Guid internalUserId
            && internalUserId != Guid.Empty)
        {
            return internalUserId;
        }

        var claimed = httpContext.User.GetClaimedUserIdOrNull();
        if (claimed.HasValue && claimed.Value != Guid.Empty)
        {
            return claimed.Value;
        }

        throw new UnauthorizedAccessException("Authenticated user context is missing an internal user id.");
    }
}
