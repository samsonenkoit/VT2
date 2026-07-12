using Database;
using Database.Models;
using Database.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Vt.Tests.Repositories;

public class TaskFileRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly VtDbContext _context;
    private readonly TaskFileRepository _repository;

    public TaskFileRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<VtDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new VtDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new TaskFileRepository(_context);
    }

    [Fact]
    public async Task GetNotDeletedAsync_ReturnsOnlyActiveFilesForTask()
    {
        var task = CreateTask("Задача");
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        _context.TaskFiles.AddRange(
            CreateFile("active.pdf", task.Id),
            CreateFile("deleted.pdf", task.Id, deletedAt: DateTime.UtcNow));
        await _context.SaveChangesAsync();

        var files = await _repository.GetNotDeletedAsync(task.Id);

        Assert.Single(files);
        Assert.Equal("active.pdf", files[0].FileName);
    }

    [Fact]
    public async Task SoftDeleteAsync_MarksFileAsDeleted()
    {
        var task = CreateTask("Задача");
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var file = CreateFile("report.pdf", task.Id);
        _context.TaskFiles.Add(file);
        await _context.SaveChangesAsync();

        await _repository.SoftDeleteAsync(file.Id);

        var files = await _repository.GetNotDeletedAsync(task.Id);
        Assert.Empty(files);
    }

    private static TaskDb CreateTask(string title)
    {
        return new TaskDb
        {
            Title = title,
            DueDateUtc = new DateTime(2026, 4, 1),
            ProgressPercent = 0,
            Priority = TaskPriority.Medium,
        };
    }

    private static TaskFileDb CreateFile(string fileName, int taskId, DateTime? deletedAt = null)
    {
        return new TaskFileDb
        {
            TaskId = taskId,
            FileName = fileName,
            StoredPath = $"TasksFiles/Task_{taskId}/{fileName}",
            DeletedAtUtc = deletedAt,
        };
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
