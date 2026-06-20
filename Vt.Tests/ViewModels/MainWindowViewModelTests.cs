using VtApp.ViewModels;
using Xunit;

namespace Vt.Tests.ViewModels;

public class MainWindowViewModelTests
{
    [Fact]
    public void Constructor_SetsHomeAsDefaultPage()
    {
        var viewModel = new MainWindowViewModel();

        Assert.Equal("Home", viewModel.SelectedPage);
        Assert.IsType<HomeViewModel>(viewModel.CurrentView);
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
    public void SelectedPage_WhenChangedToUnknownValue_SetsCurrentViewToHomeViewModel()
    {
        var viewModel = new MainWindowViewModel();
        viewModel.SelectedPage = "Unknown";

        Assert.Equal("Unknown", viewModel.SelectedPage);
        Assert.IsType<HomeViewModel>(viewModel.CurrentView);
    }

    [Fact]
    public void SelectedPage_WhenChangedBackToHome_SetsCurrentViewToHomeViewModel()
    {
        var viewModel = new MainWindowViewModel();
        viewModel.SelectedPage = "Settings";
        viewModel.SelectedPage = "Home";

        Assert.Equal("Home", viewModel.SelectedPage);
        Assert.IsType<HomeViewModel>(viewModel.CurrentView);
    }
}
