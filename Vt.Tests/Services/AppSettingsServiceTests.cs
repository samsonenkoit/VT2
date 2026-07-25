using Database;
using VtApp.Models;
using VtApp.Services;
using Xunit;

namespace Vt.Tests.Services;

public class AppSettingsServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly AppSettingsService _service;

    public AppSettingsServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "VT2_SettingsTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
        _service = new AppSettingsService(new TestAppDataPathProvider(_tempDirectory));
    }

    [Fact]
    public void Load_WhenFileMissing_ReturnsLightTheme()
    {
        var settings = _service.Load();

        Assert.Equal(AppTheme.Light, settings.Theme);
        Assert.Equal(AppTheme.Light, _service.Current.Theme);
    }

    [Fact]
    public void Save_ThenLoad_PersistsDarkTheme()
    {
        _service.Save(new AppSettings { Theme = AppTheme.Dark });

        var reloaded = new AppSettingsService(new TestAppDataPathProvider(_tempDirectory)).Load();

        Assert.Equal(AppTheme.Dark, reloaded.Theme);
        Assert.True(File.Exists(Path.Combine(_tempDirectory, "settings.json")));
        var json = File.ReadAllText(Path.Combine(_tempDirectory, "settings.json"));
        Assert.Contains("\"theme\": \"Dark\"", json);
    }

    [Fact]
    public void Load_WhenJsonIsInvalid_ReturnsLightTheme()
    {
        File.WriteAllText(Path.Combine(_tempDirectory, "settings.json"), "{ not valid json");

        var settings = _service.Load();

        Assert.Equal(AppTheme.Light, settings.Theme);
    }

    [Fact]
    public void Load_WhenThemeMissing_ReturnsLightTheme()
    {
        File.WriteAllText(Path.Combine(_tempDirectory, "settings.json"), "{}");

        var settings = _service.Load();

        Assert.Equal(AppTheme.Light, settings.Theme);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);
    }

    private sealed class TestAppDataPathProvider(string root) : IAppDataPathProvider
    {
        public string GetAppDataDirectory() => root;

        public string GetDatabaseFilePath() => Path.Combine(root, "vt2.db");

        public string GetSettingsFilePath() => Path.Combine(root, "settings.json");

        public string GetTaskFilesDirectory(int taskId) =>
            Path.Combine(root, "TasksFiles", $"Task_{taskId}");
    }
}
