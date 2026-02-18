using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PersonalFinanceTracker.IntegrationTests.TestHost;

public static class TestJwtTokenFactory
{
    public static string Create(
        Guid userId,
        bool includeScope = true,
        DateTime? expiresAtUtc = null,
        DateTime? notBeforeUtc = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("sub", userId.ToString()),
            new(ClaimTypes.Email, $"test-{userId:N}@example.com"),
            new(ClaimTypes.Name, "Integration Test User")
        };

        if (includeScope)
        {
            claims.Add(new Claim("scope", "finance-api"));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestAuthDefaults.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: TestAuthDefaults.Issuer,
            audience: TestAuthDefaults.Audience,
            claims: claims,
            notBefore: notBeforeUtc ?? DateTime.UtcNow.AddMinutes(-1),
            expires: expiresAtUtc ?? DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
