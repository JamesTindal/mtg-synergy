using Microsoft.EntityFrameworkCore;
using MtgSynergy.Domain.Entities;

namespace MtgSynergy.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Card> Cards => Set<Card>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.ScryfallId).IsUnique();
            entity.HasIndex(c => c.Name);
            entity.HasIndex(c => c.SetCode);

            entity.Property(c => c.Name).IsRequired().HasMaxLength(250);
            entity.Property(c => c.SetCode).IsRequired().HasMaxLength(20);
            entity.Property(c => c.Cmc).HasPrecision(6, 2);

            // Store list properties as Postgres native text arrays
            entity.Property(c => c.Colors).HasColumnType("text[]");
            entity.Property(c => c.ColorIdentity).HasColumnType("text[]");
            entity.Property(c => c.Keywords).HasColumnType("text[]");

            // Ignore computed properties
            entity.Ignore(c => c.MainTypes);
            entity.Ignore(c => c.Subtypes);
            entity.Ignore(c => c.IsCreature);
            entity.Ignore(c => c.IsInstant);
            entity.Ignore(c => c.IsSorcery);
            entity.Ignore(c => c.IsArtifact);
            entity.Ignore(c => c.IsEnchantment);
            entity.Ignore(c => c.IsPlaneswalker);
            entity.Ignore(c => c.IsLand);
        });
    }
}
