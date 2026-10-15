using VtApp.Models;
using VtApp.Services;
using VtApp.ViewModels;
using Xunit;

namespace Vt.Tests.ViewModels;

public class SettingsViewModelTests
{
    [Fact]
    public void VersionText_ShowsFourPartVersionFromVersionService()
    {
        var viewModel = new SettingsViewModel(
            new StubAppSettingsService(),
            new StubThemeService(),
            new StubAppVersionService(new AppVersion { Major = 1, Minor = 0 }));

        Assert.Equal("Версия: 1.0.0.0", viewModel.VersionText);
        Assert.Equal("Год выпуска: 2026", viewModel.ReleaseYearText);
    }

    private sealed class StubAppSettingsService : IAppSettingsService
    {
        public AppSettings Current { get; private set; } = new();

        public AppSettings Load() => Current;

        public void Save(AppSettings settings) => Current = settings;
    }

    private sealed class StubThemeService : IThemeService
    {
        public void Apply(AppTheme theme)
        {
        }
    }

    private sealed class StubAppVersionService(AppVersion current) : IAppVersionService
    {
        public AppVersion Current { get; } = current;

        public AppVersion Load() => Current;
    }
}
