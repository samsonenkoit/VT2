using CommunityToolkit.Mvvm.ComponentModel;

namespace VtApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly HomeViewModel _homeViewModel = new();
    private readonly SettingsViewModel _settingsViewModel = new();

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _selectedPage = "Home";

    public MainWindowViewModel()
    {
        CurrentView = _homeViewModel;
    }

    partial void OnSelectedPageChanged(string value)
    {
        CurrentView = value switch
        {
            "Home" => _homeViewModel,
            "Settings" => _settingsViewModel,
            _ => _homeViewModel
        };
    }
}
