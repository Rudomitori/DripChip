using DripChip.Entities;
using Microsoft.EntityFrameworkCore;

namespace DripChip.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<Account>();
        modelBuilder.Entity<AnimalType>();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<AnimalType2Animal>().HasKey(x => new { x.AnimalTypeId, x.AnimalId });
        modelBuilder.Entity<LocationVisit>();

        modelBuilder
            .Entity<Location>()
            .Property(b => b.Coordinates)
            .HasColumnType("geography (point)");

        base.OnModelCreating(modelBuilder);
    }
}
