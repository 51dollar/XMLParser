using Microsoft.EntityFrameworkCore;
using Shared.Entity;

namespace DataProcessorService.Data;

public class SqliteDbContext(DbContextOptions<SqliteDbContext> options) : DbContext(options)
{
    public DbSet<ModuleData> Modules => Set<ModuleData>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ModuleData>(entity =>
        {
            entity.HasKey(m => m.ModuleCategoryId);

            entity.Property(m => m.ModuleCategoryId)
                .IsRequired()
                .HasColumnType("TEXT")
                .HasMaxLength(50);

            entity.Property(m => m.ModuleState)
                .IsRequired()
                .HasColumnType("TEXT")
                .HasMaxLength(30);
        });
    }
}