using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Database;

public class VtDbContext(DbContextOptions<VtDbContext> options) : DbContext(options)
{
    public DbSet<TaskDb> Tasks => Set<TaskDb>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskDb>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(500);
            entity.Property(t => t.Priority).HasConversion<int>();
        });
    }
}
