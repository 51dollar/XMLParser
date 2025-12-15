using DataProcessorService.Models;
using Microsoft.EntityFrameworkCore;

namespace DataProcessorService.Data;

public class SqLiteDbContext(DbContextOptions<SqLiteDbContext> options) : DbContext(options)
{
    public DbSet<ModuleData> Modules => Set<ModuleData>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ModuleData>()
            .HasKey(m => m.ModuleCategoryID);
    }
}