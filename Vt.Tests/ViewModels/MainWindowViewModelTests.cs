using Database.Models;
using Database.Repositories;
using VtApp.Services;
using VtApp.ViewModels;
using Xunit;

namespace Vt.Tests.ViewModels;

public class MainWindowViewModelTests
{
    [Fact]
    public void Constructor_SetsTasksAsDefaultPage()
    {
        var viewModel = CreateViewModel();

        Assert.Equal("Tasks", viewModel.SelectedPage);
        Assert.IsType<TasksViewModel>(viewModel.CurrentView);
    }

    [Fact]
    public void SelectedPage_WhenChangedToSettings_SetsCurrentViewToSettingsViewModel()
    {
        var viewModel = CreateViewModel();
        viewModel.SelectedPage = "Settings";

        Assert.Equal("Settings", viewModel.SelectedPage);
        Assert.IsType<SettingsViewModel>(viewModel.CurrentView);
    }

    [Fact]
    public void SelectedPage_WhenChangedToUnknownValue_SetsCurrentViewToTasksViewModel()
    {
        var viewModel = CreateViewModel();
        viewModel.SelectedPage = "Unknown";

        Assert.Equal("Unknown", viewModel.SelectedPage);
        Assert.IsType<TasksViewModel>(viewModel.CurrentView);
    }

    [Fact]
    public void SelectedPage_WhenChangedBackToTasks_SetsCurrentViewToTasksViewModel()
    {
        var viewModel = CreateViewModel();
        viewModel.SelectedPage = "Settings";
        viewModel.SelectedPage = "Tasks";

        Assert.Equal("Tasks", viewModel.SelectedPage);
        Assert.IsType<TasksViewModel>(viewModel.CurrentView);
    }

    [Fact]
    public async Task SelectedPage_WhenChangedToSettings_ResetsTasksNavigation()
    {
        var tasksViewModel = CreateTasksViewModel();
        await tasksViewModel.AddTaskCommand.ExecuteAsync(null);

        var viewModel = new MainWindowViewModel(tasksViewModel, new SettingsViewModel());
        viewModel.SelectedPage = "Settings";

        Assert.Same(tasksViewModel, tasksViewModel.CurrentContent);
    }

    private static MainWindowViewModel CreateViewModel()
    {
        return new MainWindowViewModel(CreateTasksViewModel(), new SettingsViewModel());
    }

    private static TasksViewModel CreateTasksViewModel()
    {
        var repository = new EmptyTaskRepository();
        return new TasksViewModel(
            repository,
            new TaskEditViewModel(
                repository,
                new EmptySubtaskRepository(),
                new EmptyGoalRepository(),
                new EmptyTaskFileService()));
    }

    private sealed class EmptyTaskRepository : ITaskRepository
    {
        public Task<IReadOnlyList<TaskDb>> GetAllNotDeletedAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TaskDb>>([]);

        public Task<TaskDb?> GetAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult<TaskDb?>(null);

        public Task<TaskDb> AddAsync(TaskDb task, CancellationToken cancellationToken = default) =>
            Task.FromResult(task);

        public Task UpdateAsync(TaskDb task, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class EmptyTaskFileService : ITaskFileService
    {
        public Task<IReadOnlyList<VtApp.Models.TaskFileItem>> GetFilesAsync(int taskId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<VtApp.Models.TaskFileItem>>([]);

        public Task<VtApp.Models.TaskFileItem> AddFileAsync(int taskId, string sourceFilePath, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task DeleteFileAsync(int taskId, string fileName, CancellationToken cancellationToken = default) =>
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
}
