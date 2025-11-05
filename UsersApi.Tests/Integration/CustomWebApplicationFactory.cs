using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UsersApi.Data;

namespace UsersApi.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("SkipSeed", "true");
        builder.ConfigureServices(services =>
        {
            // Replace ApplicationDbContext with SQLite in-memory for tests
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            var dbPath = Path.Combine(Path.GetTempPath(), "test-users.db");
            if (File.Exists(dbPath)) File.Delete(dbPath);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Build the provider to create the database schema
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
        });
    }
}