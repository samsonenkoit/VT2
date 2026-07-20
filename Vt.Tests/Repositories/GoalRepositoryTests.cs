using Database;
using Database.Models;
using Database.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Vt.Tests.Repositories;

public class GoalRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly VtDbContext _context;
    private readonly GoalRepository _repository;

    public GoalRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<VtDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new VtDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new GoalRepository(_context);
    }

    [Fact]
    public async Task GetNotDeletedAsync_ReturnsOnlyNonDeletedForTask()
    {
        var task = CreateTask("Задача");
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        _context.Goals.AddRange(
            CreateGoal("Активная", task.Id),
            CreateGoal("Удалённая", task.Id, deletedAt: DateTime.UtcNow));
        await _context.SaveChangesAsync();

        var active = await _repository.GetNotDeletedAsync(task.Id);

        Assert.Single(active);
        Assert.Equal("Активная", active[0].Text);
    }

    [Fact]
    public async Task GetNotDeletedAsync_FiltersByTaskId()
    {
        var firstTask = CreateTask("Первая");
        var secondTask = CreateTask("Вторая");
        _context.Tasks.AddRange(firstTask, secondTask);
        await _context.SaveChangesAsync();

        _context.Goals.AddRange(
            CreateGoal("Цель 1", firstTask.Id),
            CreateGoal("Цель 2", secondTask.Id));
        await _context.SaveChangesAsync();

        var goals = await _repository.GetNotDeletedAsync(firstTask.Id);

        Assert.Single(goals);
        Assert.Equal("Цель 1", goals[0].Text);
    }

    [Fact]
    public async Task AddAsync_PersistsGoal()
    {
        var task = CreateTask("Задача");
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var created = await _repository.AddAsync(CreateGoal("Новая цель", task.Id));

        Assert.True(created.Id > 0);
        var goals = await _repository.GetNotDeletedAsync(task.Id);
        Assert.Single(goals);
        Assert.Equal("Новая цель", goals[0].Text);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var task = CreateTask("Задача");
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var goal = CreateGoal("Старая", task.Id);
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        await _repository.UpdateAsync(new GoalDb
        {
            Id = goal.Id,
            TaskId = task.Id,
            Text = "Новая",
        });

        var updated = Assert.Single(await _repository.GetNotDeletedAsync(task.Id));
        Assert.Equal("Новая", updated.Text);
    }

    [Fact]
    public async Task SoftDeleteAsync_HidesFromGetNotDeleted()
    {
        var task = CreateTask("Задача");
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var goal = CreateGoal("К удалению", task.Id);
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        await _repository.SoftDeleteAsync(goal.Id);

        Assert.Empty(await _repository.GetNotDeletedAsync(task.Id));
        Assert.NotNull(_context.Goals.Single(g => g.Id == goal.Id).DeletedAtUtc);
    }

    private static TaskDb CreateTask(string title) =>
        new()
        {
            Title = title,
            DueDateUtc = new DateTime(2026, 4, 1),
            ProgressPercent = 0,
            Priority = TaskPriority.Medium,
        };

    private static GoalDb CreateGoal(string text, int taskId, DateTime? deletedAt = null) =>
        new()
        {
            Text = text,
            TaskId = taskId,
            DeletedAtUtc = deletedAt,
        };

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
