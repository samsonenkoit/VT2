using Database;
using Database.Models;
using Database.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Vt.Tests.Repositories;

public class SubtaskRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly VtDbContext _context;
    private readonly SubtaskRepository _repository;

    public SubtaskRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<VtDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new VtDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new SubtaskRepository(_context);
    }

    [Fact]
    public async Task GetNotDeletedAsync_ReturnsOnlyNonDeletedForTask()
    {
        var task = CreateTask("Задача");
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        _context.Subtasks.AddRange(
            CreateSubtask("Активная", task.Id),
            CreateSubtask("Удалённая", task.Id, deletedAt: DateTime.UtcNow));
        await _context.SaveChangesAsync();

        var active = await _repository.GetNotDeletedAsync(task.Id);

        Assert.Single(active);
        Assert.Equal("Активная", active[0].Title);
    }

    [Fact]
    public async Task GetNotDeletedAsync_FiltersByTaskId()
    {
        var firstTask = CreateTask("Первая");
        var secondTask = CreateTask("Вторая");
        _context.Tasks.AddRange(firstTask, secondTask);
        await _context.SaveChangesAsync();

        _context.Subtasks.AddRange(
            CreateSubtask("Подзадача 1", firstTask.Id),
            CreateSubtask("Подзадача 2", secondTask.Id));
        await _context.SaveChangesAsync();

        var subtasks = await _repository.GetNotDeletedAsync(firstTask.Id);

        Assert.Single(subtasks);
        Assert.Equal("Подзадача 1", subtasks[0].Title);
    }

    [Fact]
    public async Task AddAsync_PersistsSubtask()
    {
        var task = CreateTask("Задача");
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var created = await _repository.AddAsync(CreateSubtask("Новая подзадача", task.Id));

        Assert.True(created.Id > 0);

        var subtasks = await _repository.GetNotDeletedAsync(task.Id);
        Assert.Single(subtasks);
        Assert.Equal("Новая подзадача", subtasks[0].Title);
    }

    [Fact]
    public async Task UpdateAsync_WhenEntityAlreadyTracked_UpdatesWithoutConflict()
    {
        var task = CreateTask("Задача");
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var subtask = CreateSubtask("Старое название", task.Id);
        _context.Subtasks.Add(subtask);
        await _context.SaveChangesAsync();

        await _repository.GetNotDeletedAsync(task.Id);

        await _repository.UpdateAsync(new SubtaskDb
        {
            Id = subtask.Id,
            Title = "Новое название",
            TaskId = task.Id,
        });

        var subtasks = await _repository.GetNotDeletedAsync(task.Id);

        Assert.Single(subtasks);
        Assert.Equal("Новое название", subtasks[0].Title);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var task = CreateTask("Задача");
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var subtask = CreateSubtask("Старое название", task.Id);
        _context.Subtasks.Add(subtask);
        await _context.SaveChangesAsync();

        subtask.Title = "Новое название";
        await _repository.UpdateAsync(subtask);

        var subtasks = await _repository.GetNotDeletedAsync(task.Id);

        Assert.Single(subtasks);
        Assert.Equal("Новое название", subtasks[0].Title);
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

    private static SubtaskDb CreateSubtask(string title, int taskId, DateTime? deletedAt = null)
    {
        return new SubtaskDb
        {
            Title = title,
            TaskId = taskId,
            DeletedAtUtc = deletedAt,
        };
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
