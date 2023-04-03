using DripChip.Entities;
using DripChip.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace DripChip.WebApi.Setup;

public static class DbSetup
{
    public static WebApplicationBuilder SetupDb(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(optionsBuilder =>
        {
            var connectionString = builder.Configuration.GetConnectionString("Default");
            optionsBuilder.UseNpgsql(
                connectionString,
                contextOptionsBuilder =>
                {
                    contextOptionsBuilder.UseNetTopologySuite();
                }
            );
        });
        builder.Services.AddScoped<DbContext>(
            provider => provider.GetRequiredService<AppDbContext>()
        );

        builder.Services.AddSingleton(
            new NtsGeometryServices(
                CoordinateArraySequenceFactory.Instance,
                new PrecisionModel(1000d),
                4326, // longitude and latitude
                GeometryOverlay.NG,
                new CoordinateEqualityComparer()
            )
        );

        return builder;
    }

    public static async Task ApplyMigrations(this WebApplication app)
    {
        using var serviceScope = app.Services.CreateScope();
        var appDbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await appDbContext.Database.EnsureDeletedAsync();
        await appDbContext.Database.EnsureCreatedAsync();
    }

    public static async Task SeedDb(this WebApplication app)
    {
        using var serviceScope = app.Services.CreateScope();
        var appDbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<Account>>();
        var logger = serviceScope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DbSetup");

        var rolesToSeed = await appDbContext
            .FromValues(new[] { Role.Admin, Role.Chipper, Role.User })
            .Where(x => !appDbContext.Set<Account>().Any(y => y.Role == x))
            .ToListAsync();

        foreach (var role in rolesToSeed.OrderByDescending(x => x))
        {
            var newAccount = new Account
            {
                FirstName = $"{role.ToString().ToLower()}FirstName",
                LastName = $"{role.ToString().ToLower()}LastName",
                Email = $"{role.ToString().ToLower()}@simbirsoft.com",
                Role = role
            };

            var identityResult = await userManager.CreateAsync(newAccount, "qwerty123");
            if (identityResult.Succeeded)
                logger.LogInformation(
                    "Account with email \"{email}\" and role \"{role}\" was seeded successfully",
                    newAccount.Email,
                    role
                );
            else
                logger.LogError(
                    "Account for role \"{role}\" wasn't seeded because: \n\t{errors}",
                    role,
                    string.Join(
                        "\n\t",
                        identityResult.Errors.Select(x => $"[{x.Code}] {x.Description}")
                    )
                );
        }
    }
}
