using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using UsersApi.Data;
using Xunit;

namespace UsersApi.Tests.Integration;

public class AuthFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOpts = new(JsonSerializerDefaults.Web);

    public AuthFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_Login_Me_Succeeds()
    {
        var client = _factory.CreateClient();

        // Seed roles
        using (var scope = _factory.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await roleManager.CreateAsync(new IdentityRole("User"));
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Register
        var registerBody = JsonContent("username","bob","password","Pass!234","role","User");
        var regResp = await client.PostAsync("/api/auth/register", registerBody);
        regResp.EnsureSuccessStatusCode();

        // Login
        var loginBody = JsonContent("username","bob","password","Pass!234");
        var loginResp = await client.PostAsync("/api/auth/login", loginBody);
        loginResp.EnsureSuccessStatusCode();
        var token = await ExtractTokenAsync(loginResp);

        // Me
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var meResp = await client.GetAsync("/api/users/me");
        meResp.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AdminOnly_Forbidden_For_User_Then_Allowed_For_Admin()
    {
        var client = _factory.CreateClient();

        // Seed roles and admin
        using (var scope = _factory.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            await roleManager.CreateAsync(new IdentityRole("User"));
            await roleManager.CreateAsync(new IdentityRole("Admin"));
            var admin = new IdentityUser { UserName = "admin" };
            await userManager.CreateAsync(admin, "Adm1n!234");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Register normal user
        var registerBody = JsonContent("username","carol","password","Pass!234","role","User");
        var regResp = await client.PostAsync("/api/auth/register", registerBody);
        regResp.EnsureSuccessStatusCode();

        // Login as normal user
        var loginBody = JsonContent("username","carol","password","Pass!234");
        var loginResp = await client.PostAsync("/api/auth/login", loginBody);
        loginResp.EnsureSuccessStatusCode();
        var userToken = await ExtractTokenAsync(loginResp);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var forbiddenResp = await client.GetAsync("/api/users/admin-only");
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, forbiddenResp.StatusCode);

        // Login as admin
        var adminLoginBody = JsonContent("username","admin","password","Adm1n!234");
        var adminLoginResp = await client.PostAsync("/api/auth/login", adminLoginBody);
        adminLoginResp.EnsureSuccessStatusCode();
        var adminToken = await ExtractTokenAsync(adminLoginResp);

        // Assign role Admin to user
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var assignResp = await client.PostAsync("/api/auth/assign-role?username=carol&role=Admin", null);
        assignResp.EnsureSuccessStatusCode();

        // Login again as carol and access admin-only
        loginResp = await client.PostAsync("/api/auth/login", loginBody);
        loginResp.EnsureSuccessStatusCode();
        userToken = await ExtractTokenAsync(loginResp);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var okResp = await client.GetAsync("/api/users/admin-only");
        okResp.EnsureSuccessStatusCode();
    }

    private StringContent JsonContent(params string[] kv)
    {
        var dict = new Dictionary<string, string>();
        for (int i = 0; i < kv.Length; i += 2)
        {
            dict[kv[i]] = kv[i + 1];
        }
        var json = JsonSerializer.Serialize(dict, _jsonOpts);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private async Task<string> ExtractTokenAsync(HttpResponseMessage resp)
    {
        using var stream = await resp.Content.ReadAsStreamAsync();
        var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.GetProperty("token").GetString()!;
    }
}