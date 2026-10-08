using Microsoft.EntityFrameworkCore;
using oBiletCase.Infrastructure.Localization;

namespace oBiletCase.Infrastructure.Persistence;

internal sealed class AppDbContext : DbContext
{
    public const string ConnectionStringName = "oBiletCaseDb";

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<LocalizationResource> LocalizationResources => Set<LocalizationResource>();

    public DbSet<FeatureTranslation> FeatureTranslations => Set<FeatureTranslation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LocalizationResource>(entity =>
        {
            entity.HasKey(r => new { r.Key, r.Culture });
            entity.Property(r => r.Key).HasMaxLength(128);
            entity.Property(r => r.Culture).HasMaxLength(8);
            entity.Property(r => r.Value).IsRequired();
            entity.HasData(LocalizationSeedData.Resources);
        });

        modelBuilder.Entity<FeatureTranslation>(entity =>
        {
            entity.HasKey(t => new { t.FeatureId, t.Culture });
            entity.Property(t => t.FeatureId).ValueGeneratedNever();
            entity.Property(t => t.Culture).HasMaxLength(8);
            entity.Property(t => t.Name).HasMaxLength(256).IsRequired();
            entity.HasData(LocalizationSeedData.FeatureTranslations);
        });
    }
}
