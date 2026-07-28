using CommunityToolkit.Mvvm.ComponentModel;
using VtApp.Models;
using VtApp.Services;

namespace VtApp.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IAppSettingsService _settingsService;
    private readonly IThemeService _themeService;

    [ObservableProperty]
    private AppTheme _selectedTheme;

    public SettingsViewModel(
        IAppSettingsService settingsService,
        IThemeService themeService,
        IAppVersionService versionService)
    {
        _settingsService = settingsService;
        _themeService = themeService;
        // Assign field to avoid Save/Apply on construction.
        _selectedTheme = settingsService.Current.Theme;
        VersionText = $"Версия {versionService.Current.Display}";
    }

    public string Title => "Настройки";

    public string VersionText { get; }

    partial void OnSelectedThemeChanged(AppTheme value)
    {
        var settings = new AppSettings { Theme = value };
        _settingsService.Save(settings);
        _themeService.Apply(value);
    }
}
