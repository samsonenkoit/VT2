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

    private static MainWindowViewModel CreateViewModel()
    {
        var tasksViewModel = new TasksViewModel(new EmptyTaskRepository());
        return new MainWindowViewModel(tasksViewModel, new SettingsViewModel());
    }

    private sealed class EmptyTaskRepository : ITaskRepository
    {
        public Task<IReadOnlyList<TaskDb>> GetAllActiveAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TaskDb>>([]);

        public Task UpdateAsync(TaskDb task, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
