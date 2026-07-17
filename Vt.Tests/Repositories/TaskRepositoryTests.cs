using Database;
using Database.Models;
using Database.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Vt.Tests.Repositories;

public class TaskRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly VtDbContext _context;
    private readonly TaskRepository _repository;

    public TaskRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<VtDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new VtDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new TaskRepository(_context);
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsOnlyNonDeletedTasks()
    {
        _context.Tasks.AddRange(
            CreateTask("Активная"),
            CreateTask("Удалённая", deletedAt: DateTime.UtcNow));
        await _context.SaveChangesAsync();

        var active = await _repository.GetAllNotDeletedAsync();

        Assert.Single(active);
        Assert.Equal("Активная", active[0].Title);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var task = CreateTask("Старое название", progressPercent: 10);
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        task.Title = "Новое название";
        task.ProgressPercent = 75;
        await _repository.UpdateAsync(task);

        var active = await _repository.GetAllNotDeletedAsync();

        Assert.Single(active);
        Assert.Equal("Новое название", active[0].Title);
        Assert.Equal(75, active[0].ProgressPercent);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTask()
    {
        var task = CreateTask("Найти по Id");
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var found = await _repository.GetAsync(task.Id);

        Assert.NotNull(found);
        Assert.Equal("Найти по Id", found.Title);
    }

    [Fact]
    public async Task AddAsync_PersistsTask()
    {
        var task = CreateTask("Новая задача");

        var created = await _repository.AddAsync(task);

        Assert.True(created.Id > 0);

        var found = await _repository.GetAsync(created.Id);
        Assert.NotNull(found);
        Assert.Equal("Новая задача", found.Title);
    }

    private static TaskDb CreateTask(
        string title,
        TaskPriority priority = TaskPriority.Medium,
        int progressPercent = 0,
        DateTime? deletedAt = null)
    {
        return new TaskDb
        {
            Title = title,
            DueDateUtc = new DateTime(2026, 4, 1),
            ProgressPercent = progressPercent,
            Priority = priority,
            DeletedAtUtc = deletedAt,
        };
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
