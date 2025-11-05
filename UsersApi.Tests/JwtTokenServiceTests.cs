using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using UsersApi.Services;
using Xunit;

namespace UsersApi.Tests;

public class JwtTokenServiceTests
{
    [Fact]
    public void GenerateToken_IncludesRoleClaim()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "TEST_SECRET_KEY_123456789012345678901234567890" },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" },
            { "Jwt:ExpirationMinutes", "5" }
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();
        var service = new JwtTokenService(config);

        var user = new IdentityUser { Id = "user-1", UserName = "alice" };
        var roles = new List<string> { "User" };

        var tokenString = service.GenerateToken(user, roles);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        Assert.Contains(token.Claims, c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "User");
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.UniqueName && c.Value == "alice");
    }
}