using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Database;

public class VtDbContext(DbContextOptions<VtDbContext> options) : DbContext(options)
{
    public DbSet<TaskDb> Tasks => Set<TaskDb>();

    public DbSet<SubtaskDb> Subtasks => Set<SubtaskDb>();

    public DbSet<TaskFileDb> TaskFiles => Set<TaskFileDb>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskDb>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(500);
            entity.Property(t => t.Priority).HasConversion<int>();
            entity.Property(t => t.Importance).HasConversion<int>();
            entity.Property(t => t.DelayRisk).HasConversion<int>();
            entity.Property(t => t.Difficulty).HasConversion<int>();
            entity.Property(t => t.Urgency).HasConversion<int>();

            entity.HasMany(t => t.Subtasks)
                .WithOne(s => s.Task)
                .HasForeignKey(s => s.TaskId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SubtaskDb>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Title).IsRequired().HasMaxLength(500);
        });

        modelBuilder.Entity<TaskFileDb>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.FileName).IsRequired().HasMaxLength(500);
            entity.Property(f => f.StoredPath).IsRequired().HasMaxLength(1000);

            entity.HasOne(f => f.Task)
                .WithMany()
                .HasForeignKey(f => f.TaskId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
