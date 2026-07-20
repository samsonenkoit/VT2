using Database.Models;
using Database.Repositories;
using VtApp.Models;
using VtApp.Services;
using VtApp.ViewModels;
using Xunit;

namespace Vt.Tests.ViewModels;

public class TasksViewModelTests
{
    [Fact]
    public async Task LoadTasksAsync_PopulatesColumnsByPriority()
    {
        var viewModel = CreateViewModel(new FakeTaskRepository(
        [
            CreateTask(1, "Критическая", TaskPriority.Critical),
            CreateTask(2, "Срочная", TaskPriority.Urgent),
            CreateTask(3, "Средняя", TaskPriority.Medium),
            CreateTask(4, "Несрочная", TaskPriority.NotUrgent),
        ]));

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
        var viewModel = CreateViewModel(new FakeTaskRepository([]));

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
        var viewModel = CreateViewModel(new FakeTaskRepository(
        [
            CreateTask(1, "Из БД", TaskPriority.Medium, progressPercent: 42),
        ]));

        await viewModel.LoadTasksAsync();

        var task = Assert.Single(viewModel.MediumTasks);
        Assert.Equal(1, task.Id);
        Assert.Equal(0, task.EmailCount);
        Assert.Empty(task.BadgeCounts);
        Assert.Equal(42, task.ProgressPercent);
    }

    [Fact]
    public async Task AddTask_SetsCurrentContentToEditViewModel()
    {
        var viewModel = CreateViewModel(new FakeTaskRepository([]));

        await viewModel.AddTaskCommand.ExecuteAsync(null);

        Assert.IsType<TaskEditViewModel>(viewModel.CurrentContent);
        var editViewModel = (TaskEditViewModel)viewModel.CurrentContent;
        Assert.False(editViewModel.IsEditMode);
        Assert.False(editViewModel.IsLoading);
    }

    [Fact]
    public async Task EditTask_SetsCurrentContentToEditViewModel()
    {
        var viewModel = CreateViewModel(new FakeTaskRepository(
        [
            CreateTask(5, "Редактировать", TaskPriority.Medium),
        ]));

        await viewModel.LoadTasksAsync();
        var task = viewModel.MediumTasks.Single();

        await viewModel.EditTaskCommand.ExecuteAsync(task);

        Assert.IsType<TaskEditViewModel>(viewModel.CurrentContent);
        var editViewModel = (TaskEditViewModel)viewModel.CurrentContent;
        Assert.True(editViewModel.IsEditMode);
        Assert.Equal("Редактировать", editViewModel.Title);
    }

    [Fact]
    public async Task EditTask_WhenTaskMissing_ReturnsToBoard()
    {
        var viewModel = CreateViewModel(new FakeTaskRepository([]));

        await viewModel.EditTaskCommand.ExecuteAsync(new TaskItem { Id = 99, Title = "Нет" });

        Assert.Same(viewModel, viewModel.CurrentContent);
    }

    [Fact]
    public async Task ResetToBoard_SetsCurrentContentToTasksViewModel()
    {
        var viewModel = CreateViewModel(new FakeTaskRepository([]));
        await viewModel.AddTaskCommand.ExecuteAsync(null);

        viewModel.ResetToBoard();

        Assert.Same(viewModel, viewModel.CurrentContent);
    }

    private static TasksViewModel CreateViewModel(FakeTaskRepository repository)
    {
        var editViewModel = new TaskEditViewModel(
            repository,
            new EmptySubtaskRepository(),
            new EmptyTaskFileService());
        var tasksViewModel = new TasksViewModel(repository, editViewModel);
        return tasksViewModel;
    }

    private static TaskDb CreateTask(
        int id,
        string title,
        TaskPriority priority,
        int progressPercent = 0) =>
        new()
        {
            Id = id,
            Title = title,
            DueDateUtc = new DateTime(2026, 4, 1),
            ProgressPercent = progressPercent,
            Priority = priority,
        };

    private sealed class EmptyTaskFileService : ITaskFileService
    {
        public Task<IReadOnlyList<VtApp.Models.TaskFileItem>> GetFilesAsync(int taskId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<VtApp.Models.TaskFileItem>>([]);

        public Task<VtApp.Models.TaskFileItem> AddFileAsync(int taskId, string sourceFilePath, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task DeleteFileAsync(int fileId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class EmptySubtaskRepository : ISubtaskRepository
    {
        public Task<IReadOnlyList<SubtaskDb>> GetNotDeletedAsync(int taskId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<SubtaskDb>>([]);

        public Task<SubtaskDb> AddAsync(SubtaskDb subtask, CancellationToken cancellationToken = default) =>
            Task.FromResult(subtask);

        public Task UpdateAsync(SubtaskDb subtask, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeTaskRepository : ITaskRepository
    {
        private readonly IReadOnlyList<TaskDb> _tasks;

        public FakeTaskRepository(IReadOnlyList<TaskDb> tasks) => _tasks = tasks;

        public Task<IReadOnlyList<TaskDb>> GetAllNotDeletedAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_tasks);

        public Task<TaskDb?> GetAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_tasks.FirstOrDefault(t => t.Id == id));

        public Task<TaskDb> AddAsync(TaskDb task, CancellationToken cancellationToken = default) =>
            Task.FromResult(task);

        public Task UpdateAsync(TaskDb task, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
