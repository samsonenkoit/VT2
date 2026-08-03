using System.Windows;
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
            CreateTask(5, "Редактировать", TaskPriority.Urgent),
        ]));

        await viewModel.EditTaskCommand.ExecuteAsync(new TaskItem
        {
            Id = 5,
            Title = "Редактировать",
            Priority = TaskPriority.Urgent,
        });

        Assert.IsType<TaskEditViewModel>(viewModel.CurrentContent);
        var editViewModel = (TaskEditViewModel)viewModel.CurrentContent;
        Assert.True(editViewModel.IsEditMode);
        Assert.Equal("Редактировать", editViewModel.Title);
    }

    [Fact]
    public async Task EditTask_WhenTaskMissing_StaysOnBoard()
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

    [Fact]
    public async Task DeleteTask_WhenConfirmed_RemovesFromBoardAndSoftDeletes()
    {
        var repository = new FakeTaskRepository(
        [
            CreateTask(1, "Удалить меня", TaskPriority.Medium),
            CreateTask(2, "Оставить", TaskPriority.Medium),
        ]);
        var fileService = new TrackingTaskFileService();
        var viewModel = CreateViewModel(repository, fileService);
        viewModel.ConfirmDelete = static (_, _) => MessageBoxResult.OK;
        await viewModel.LoadTasksAsync();
        var taskToDelete = viewModel.MediumTasks.First(t => t.Id == 1);

        await viewModel.DeleteTaskCommand.ExecuteAsync(taskToDelete);

        Assert.Single(viewModel.MediumTasks);
        Assert.Equal(2, viewModel.MediumTasks[0].Id);
        Assert.Equal([1], repository.SoftDeletedIds);
        Assert.Equal([1], fileService.DeletedDirectoryTaskIds);
    }

    [Fact]
    public async Task DeleteTask_WhenCancelled_DoesNothing()
    {
        var repository = new FakeTaskRepository(
        [
            CreateTask(1, "Останется", TaskPriority.Urgent),
        ]);
        var fileService = new TrackingTaskFileService();
        var viewModel = CreateViewModel(repository, fileService);
        viewModel.ConfirmDelete = static (_, _) => MessageBoxResult.Cancel;
        await viewModel.LoadTasksAsync();
        var task = Assert.Single(viewModel.UrgentTasks);

        await viewModel.DeleteTaskCommand.ExecuteAsync(task);

        Assert.Single(viewModel.UrgentTasks);
        Assert.Empty(repository.SoftDeletedIds);
        Assert.Empty(fileService.DeletedDirectoryTaskIds);
    }

    private static TasksViewModel CreateViewModel(
        FakeTaskRepository repository,
        ITaskFileService? fileService = null)
    {
        fileService ??= new EmptyTaskFileService();
        var editViewModel = new TaskEditViewModel(
            repository,
            new EmptySubtaskRepository(),
            new EmptyGoalRepository(),
            fileService);
        return new TasksViewModel(repository, fileService, editViewModel);
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
        public Task<IReadOnlyList<TaskFileItem>> GetFilesAsync(int taskId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TaskFileItem>>([]);

        public Task<TaskFileItem> AddFileAsync(int taskId, string sourceFilePath, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task DeleteFileAsync(int taskId, string fileName, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task DeleteTaskDirectoryAsync(int taskId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void OpenFile(int taskId, string fileName)
        {
        }
    }

    private sealed class TrackingTaskFileService : ITaskFileService
    {
        public List<int> DeletedDirectoryTaskIds { get; } = [];

        public Task<IReadOnlyList<TaskFileItem>> GetFilesAsync(int taskId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TaskFileItem>>([]);

        public Task<TaskFileItem> AddFileAsync(int taskId, string sourceFilePath, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task DeleteFileAsync(int taskId, string fileName, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task DeleteTaskDirectoryAsync(int taskId, CancellationToken cancellationToken = default)
        {
            DeletedDirectoryTaskIds.Add(taskId);
            return Task.CompletedTask;
        }

        public void OpenFile(int taskId, string fileName)
        {
        }
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

    private sealed class EmptyGoalRepository : IGoalRepository
    {
        public Task<IReadOnlyList<GoalDb>> GetNotDeletedAsync(int taskId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<GoalDb>>([]);

        public Task<GoalDb> AddAsync(GoalDb goal, CancellationToken cancellationToken = default) =>
            Task.FromResult(goal);

        public Task UpdateAsync(GoalDb goal, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeTaskRepository : ITaskRepository
    {
        private readonly IReadOnlyList<TaskDb> _tasks;

        public FakeTaskRepository(IReadOnlyList<TaskDb> tasks) => _tasks = tasks;

        public List<int> SoftDeletedIds { get; } = [];

        public Task<IReadOnlyList<TaskDb>> GetAllNotDeletedAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TaskDb>>(
                _tasks.Where(t => t.DeletedAtUtc is null && !SoftDeletedIds.Contains(t.Id)).ToList());

        public Task<TaskDb?> GetAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_tasks.FirstOrDefault(t => t.Id == id));

        public Task<TaskDb> AddAsync(TaskDb task, CancellationToken cancellationToken = default) =>
            Task.FromResult(task);

        public Task UpdateAsync(TaskDb task, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            SoftDeletedIds.Add(id);
            return Task.CompletedTask;
        }
    }
}
