using CommunityToolkit.Mvvm.ComponentModel;

namespace VtApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly TasksViewModel _tasksViewModel;
    private readonly SettingsViewModel _settingsViewModel;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _selectedPage = "Tasks";

    public MainWindowViewModel(TasksViewModel tasksViewModel, SettingsViewModel settingsViewModel)
    {
        _tasksViewModel = tasksViewModel;
        _settingsViewModel = settingsViewModel;
        CurrentView = _tasksViewModel;
        _ = _tasksViewModel.LoadTasksAsync();
    }

    partial void OnSelectedPageChanged(string value)
    {
        if (value != "Tasks")
            _tasksViewModel.ResetToBoard();

        CurrentView = value switch
        {
            "Tasks" => _tasksViewModel,
            "Settings" => _settingsViewModel,
            _ => _tasksViewModel
        };
    }
}
