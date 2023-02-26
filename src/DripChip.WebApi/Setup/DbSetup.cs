using DripChip.Persistence;
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
}
