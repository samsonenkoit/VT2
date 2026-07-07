using Database.Models;
using Database.Repositories;
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
    public void SelectedPage_WhenChangedToSettings_ResetsTasksNavigation()
    {
        var tasksViewModel = CreateTasksViewModel();
        tasksViewModel.AddTaskCommand.Execute(null);

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
        return new TasksViewModel(repository, new TaskEditViewModel(repository));
    }

    private sealed class EmptyTaskRepository : ITaskRepository
    {
        public Task<IReadOnlyList<TaskDb>> GetAllActiveAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TaskDb>>([]);

        public Task<TaskDb?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult<TaskDb?>(null);

        public Task<TaskDb> AddAsync(TaskDb task, CancellationToken cancellationToken = default) =>
            Task.FromResult(task);

        public Task UpdateAsync(TaskDb task, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
