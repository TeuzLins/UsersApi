using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace UsersApi.Data;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var rolesToEnsure = new[] { "Admin", "User" };
        foreach (var role in rolesToEnsure)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed opcional do usuário admin via variáveis de ambiente
        var adminUser = Environment.GetEnvironmentVariable("ADMIN_USER") ?? string.Empty;
        var adminPass = Environment.GetEnvironmentVariable("ADMIN_PASS") ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(adminUser) && !string.IsNullOrWhiteSpace(adminPass))
        {
            var existing = await userManager.FindByNameAsync(adminUser);
            if (existing is null)
            {
                var user = new IdentityUser { UserName = adminUser, Email = $"{adminUser}@local" };
                var create = await userManager.CreateAsync(user, adminPass);
                if (create.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}