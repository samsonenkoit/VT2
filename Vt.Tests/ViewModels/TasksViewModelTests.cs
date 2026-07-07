using Database.Models;
using Database.Repositories;
using VtApp.ViewModels;
using Xunit;

namespace Vt.Tests.ViewModels;

public class TasksViewModelTests
{
    [Fact]
    public async Task LoadTasksAsync_PopulatesColumnsByPriority()
    {
        var repository = new FakeTaskRepository(
        [
            CreateTask("Критическая", TaskPriority.Critical),
            CreateTask("Срочная", TaskPriority.Urgent),
            CreateTask("Средняя", TaskPriority.Medium),
            CreateTask("Несрочная", TaskPriority.NotUrgent),
        ]);
        var viewModel = new TasksViewModel(repository);

        await viewModel.LoadTasksAsync();

        Assert.False(viewModel.IsLoading);
        Assert.Single(viewModel.CriticalTasks);
        Assert.Single(viewModel.UrgentTasks);
        Assert.Single(viewModel.MediumTasks);
        Assert.Single(viewModel.NotUrgentTasks);
        Assert.Equal("Критическая", viewModel.CriticalTasks[0].Title);
    }

    [Fact]
    public async Task LoadTasksAsync_WhenNoTasks_CollectionsAreEmpty()
    {
        var viewModel = new TasksViewModel(new FakeTaskRepository([]));

        await viewModel.LoadTasksAsync();

        Assert.False(viewModel.IsLoading);
        Assert.Empty(viewModel.CriticalTasks);
        Assert.Empty(viewModel.UrgentTasks);
        Assert.Empty(viewModel.MediumTasks);
        Assert.Empty(viewModel.NotUrgentTasks);
    }

    [Fact]
    public async Task LoadTasksAsync_MapsTaskWithoutEmailAndBadges()
    {
        var viewModel = new TasksViewModel(new FakeTaskRepository(
        [
            CreateTask("Из БД", TaskPriority.Medium, progressPercent: 42),
        ]));

        await viewModel.LoadTasksAsync();

        var task = Assert.Single(viewModel.MediumTasks);
        Assert.Equal(0, task.EmailCount);
        Assert.Empty(task.BadgeCounts);
        Assert.Equal(42, task.ProgressPercent);
    }

    private static TaskDb CreateTask(string title, TaskPriority priority, int progressPercent = 0) =>
        new()
        {
            Title = title,
            DueDate = new DateTime(2026, 4, 1),
            ProgressPercent = progressPercent,
            Priority = priority,
        };

    private sealed class FakeTaskRepository(IReadOnlyList<TaskDb> tasks) : ITaskRepository
    {
        public Task<IReadOnlyList<TaskDb>> GetAllActiveAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(tasks);

        public Task UpdateAsync(TaskDb task, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
