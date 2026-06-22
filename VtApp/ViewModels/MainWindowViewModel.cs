using CommunityToolkit.Mvvm.ComponentModel;

namespace VtApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly TasksViewModel _tasksViewModel = new();
    private readonly SettingsViewModel _settingsViewModel = new();

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _selectedPage = "Tasks";

    public MainWindowViewModel()
    {
        CurrentView = _tasksViewModel;
    }

    partial void OnSelectedPageChanged(string value)
    {
        CurrentView = value switch
        {
            "Tasks" => _tasksViewModel,
            "Settings" => _settingsViewModel,
            _ => _tasksViewModel
        };
    }
}
