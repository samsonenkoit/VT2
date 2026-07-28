using VtApp.Services;
using Xunit;

namespace Vt.Tests.Services;

public class AppVersionServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _filePath;

    public AppVersionServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "VT2_VersionTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
        _filePath = Path.Combine(_tempDirectory, "version.json");
    }

    [Fact]
    public void Load_WhenFileMissing_ReturnsDefaultVersion()
    {
        var service = new AppVersionService(_filePath);

        var version = service.Load();

        Assert.Equal(0, version.Major);
        Assert.Equal(1, version.Minor);
        Assert.Equal("0.1", version.Display);
        Assert.Equal("0.1", service.Current.Display);
    }

    [Fact]
    public void Load_WhenFileValid_ReturnsVersionFromFile()
    {
        File.WriteAllText(_filePath, """{"major": 2, "minor": 5}""");
        var service = new AppVersionService(_filePath);

        var version = service.Load();

        Assert.Equal(2, version.Major);
        Assert.Equal(5, version.Minor);
        Assert.Equal("2.5", version.Display);
    }

    [Fact]
    public void Load_WhenJsonIsInvalid_ReturnsDefaultVersion()
    {
        File.WriteAllText(_filePath, "{ not valid json");
        var service = new AppVersionService(_filePath);

        var version = service.Load();

        Assert.Equal(0, version.Major);
        Assert.Equal(1, version.Minor);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);
    }
}
