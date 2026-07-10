using Database;
using Database.Repositories;
using Database.Seed;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Vt.Tests.Database;

public class DatabaseInitializerTests : IDisposable
{
    private readonly string _tempDirectory;

    public DatabaseInitializerTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "VT2.Tests", Guid.NewGuid().ToString());
    }

    [Fact]
    public void Initialize_WhenDatabaseFileMissing_SeedsTasks()
    {
        var pathProvider = new TestDatabasePathProvider(_tempDirectory);
        var initializer = new DatabaseInitializer(pathProvider);

        initializer.Run();

        using (var context = CreateContext(pathProvider.GetDatabaseFilePath()))
        {
            var taskCount = context.Tasks.Count(t => t.DeletedAtUtc == null);
            Assert.Equal(TaskSeedData.GetSeedData().Tasks.Count, taskCount);
        }
    }

    [Fact]
    public void Initialize_WhenDatabaseFileExists_DoesNotSeedAgain()
    {
        var pathProvider = new TestDatabasePathProvider(_tempDirectory);
        var initializer = new DatabaseInitializer(pathProvider);

        initializer.Run();

        using (var context = CreateContext(pathProvider.GetDatabaseFilePath()))
        {
            context.Subtasks.RemoveRange(context.Subtasks);
            context.Tasks.RemoveRange(context.Tasks);
            context.SaveChanges();
        }

        initializer.Run();

        using (var contextAfterSecondInit = CreateContext(pathProvider.GetDatabaseFilePath()))
        {
            var taskCount = contextAfterSecondInit.Tasks.Count();
            Assert.Equal(0, taskCount);
        }
    }

    [Fact]
    public async Task Initialize_CreatesSchema()
    {
        var pathProvider = new TestDatabasePathProvider(_tempDirectory);
        var initializer = new DatabaseInitializer(pathProvider);

        initializer.Run();

        await using var context = CreateContext(pathProvider.GetDatabaseFilePath());
        var repository = new TaskRepository(context);
        var activeTasks = await repository.GetAllActiveAsync();

        Assert.Equal(TaskSeedData.GetSeedData().Tasks.Count, activeTasks.Count);
    }

    private static VtDbContext CreateContext(string databasePath)
    {
        var options = new DbContextOptionsBuilder<VtDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        return new VtDbContext(options);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();

        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);
    }

    private sealed class TestDatabasePathProvider(string directory) : IDatabasePathProvider
    {
        public string GetDatabaseFilePath() => Path.Combine(directory, "vt2.db");
    }
}
