using Database.Seed;
using Microsoft.EntityFrameworkCore;

namespace Database;

public sealed class DatabaseInitializer(IAppDataPathProvider pathProvider)
{
    public void Run()
    {
        var databasePath = pathProvider.GetDatabaseFilePath();

        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

        var options = new DbContextOptionsBuilder<VtDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        using var context = new VtDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var (tasks, subtasks) = TaskSeedData.GetSeedData();
        context.Tasks.AddRange(tasks);
        context.Subtasks.AddRange(subtasks);
        context.SaveChanges();
    }

    public static void Initialize()
    {
        new DatabaseInitializer(new AppDataPathProvider()).Run();
    }
}
