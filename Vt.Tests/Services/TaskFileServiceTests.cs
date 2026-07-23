using Database;
using VtApp.Services;
using Xunit;

namespace Vt.Tests.Services;

public class TaskFileServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly TaskFileService _service;

    public TaskFileServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "VT2.Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        _service = new TaskFileService(new TestAppDataPathProvider(_tempDirectory));
    }

    [Fact]
    public async Task AddFileAsync_MovesFileIntoTasksFilesFolder()
    {
        const int taskId = 1;
        var sourcePath = Path.Combine(_tempDirectory, "source.txt");
        await File.WriteAllTextAsync(sourcePath, "test");

        var file = await _service.AddFileAsync(taskId, sourcePath);

        var expectedPath = Path.Combine(_tempDirectory, "TasksFiles", $"Task_{taskId}", "source.txt");
        Assert.True(File.Exists(expectedPath));
        Assert.False(File.Exists(sourcePath));
        Assert.Equal("source.txt", file.FileName);
        Assert.Equal("test", await File.ReadAllTextAsync(expectedPath));
    }

    [Fact]
    public async Task AddFileAsync_RenamesDuplicateFile()
    {
        const int taskId = 2;
        var firstSource = Path.Combine(_tempDirectory, "first", "report.pdf");
        var secondSource = Path.Combine(_tempDirectory, "second", "report.pdf");
        Directory.CreateDirectory(Path.GetDirectoryName(firstSource)!);
        Directory.CreateDirectory(Path.GetDirectoryName(secondSource)!);
        await File.WriteAllTextAsync(firstSource, "first");
        await File.WriteAllTextAsync(secondSource, "second");

        await _service.AddFileAsync(taskId, firstSource);
        var renamed = await _service.AddFileAsync(taskId, secondSource);

        Assert.Equal("report (1).pdf", renamed.FileName);
        Assert.True(File.Exists(Path.Combine(_tempDirectory, "TasksFiles", $"Task_{taskId}", "report.pdf")));
        Assert.True(File.Exists(Path.Combine(_tempDirectory, "TasksFiles", $"Task_{taskId}", "report (1).pdf")));
    }

    [Fact]
    public async Task GetFilesAsync_ReturnsFilesFromDisk()
    {
        const int taskId = 3;
        var taskDirectory = Path.Combine(_tempDirectory, "TasksFiles", $"Task_{taskId}");
        Directory.CreateDirectory(taskDirectory);
        await File.WriteAllTextAsync(Path.Combine(taskDirectory, "b.txt"), "b");
        await File.WriteAllTextAsync(Path.Combine(taskDirectory, "a.txt"), "a");

        var files = await _service.GetFilesAsync(taskId);

        Assert.Equal(["a.txt", "b.txt"], files.Select(f => f.FileName).ToArray());
    }

    [Fact]
    public async Task GetFilesAsync_ReturnsEmpty_WhenFolderMissing()
    {
        var files = await _service.GetFilesAsync(99);

        Assert.Empty(files);
    }

    [Fact]
    public async Task DeleteFileAsync_RemovesFileFromDisk()
    {
        const int taskId = 4;
        var sourcePath = Path.Combine(_tempDirectory, "keep.txt");
        await File.WriteAllTextAsync(sourcePath, "data");
        var file = await _service.AddFileAsync(taskId, sourcePath);
        var storedPath = Path.Combine(_tempDirectory, "TasksFiles", $"Task_{taskId}", file.FileName);

        await _service.DeleteFileAsync(taskId, file.FileName);

        Assert.False(File.Exists(storedPath));
        Assert.Empty(await _service.GetFilesAsync(taskId));
    }

    [Fact]
    public async Task DeleteFileAsync_NoOps_WhenFileMissing()
    {
        await _service.DeleteFileAsync(5, "missing.txt");
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

        public string GetTaskFilesDirectory(int taskId) =>
            Path.Combine(root, "TasksFiles", $"Task_{taskId}");
    }
}
