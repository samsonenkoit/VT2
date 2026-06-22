using VtApp.ViewModels;
using Xunit;

namespace Vt.Tests.ViewModels;

public class MainWindowViewModelTests
{
    [Fact]
    public void Constructor_SetsTasksAsDefaultPage()
    {
        var viewModel = new MainWindowViewModel();

        Assert.Equal("Tasks", viewModel.SelectedPage);
        Assert.IsType<TasksViewModel>(viewModel.CurrentView);
    }

    [Fact]
    public void SelectedPage_WhenChangedToSettings_SetsCurrentViewToSettingsViewModel()
    {
        var viewModel = new MainWindowViewModel();
        viewModel.SelectedPage = "Settings";

        Assert.Equal("Settings", viewModel.SelectedPage);
        Assert.IsType<SettingsViewModel>(viewModel.CurrentView);
    }

    [Fact]
    public void SelectedPage_WhenChangedToUnknownValue_SetsCurrentViewToTasksViewModel()
    {
        var viewModel = new MainWindowViewModel();
        viewModel.SelectedPage = "Unknown";

        Assert.Equal("Unknown", viewModel.SelectedPage);
        Assert.IsType<TasksViewModel>(viewModel.CurrentView);
    }

    [Fact]
    public void SelectedPage_WhenChangedBackToTasks_SetsCurrentViewToTasksViewModel()
    {
        var viewModel = new MainWindowViewModel();
        viewModel.SelectedPage = "Settings";
        viewModel.SelectedPage = "Tasks";

        Assert.Equal("Tasks", viewModel.SelectedPage);
        Assert.IsType<TasksViewModel>(viewModel.CurrentView);
    }
}
