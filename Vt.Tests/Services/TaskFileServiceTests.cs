using Database;
using Database.Models;
using Database.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VtApp.Services;
using Xunit;

namespace Vt.Tests.Services;

public class TaskFileServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly SqliteConnection _connection;
    private readonly VtDbContext _context;
    private readonly TaskFileService _service;

    public TaskFileServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "VT2.Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<VtDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new VtDbContext(options);
        _context.Database.EnsureCreated();
        _service = new TaskFileService(new TaskFileRepository(_context), new TestAppDataPathProvider(_tempDirectory));
    }

    [Fact]
    public async Task AddFileAsync_CopiesFileIntoTasksFilesFolder()
    {
        var task = await CreateTaskAsync("Задача");
        var sourcePath = Path.Combine(_tempDirectory, "source.txt");
        await File.WriteAllTextAsync(sourcePath, "test");

        var file = await _service.AddFileAsync(task.Id, sourcePath);

        var expectedDirectory = Path.Combine(_tempDirectory, "TasksFiles", $"Task_{task.Id}");
        var expectedPath = Path.Combine(expectedDirectory, "source.txt");
        Assert.True(File.Exists(expectedPath));
        Assert.Equal("source.txt", file.FileName);
        Assert.Equal($"TasksFiles/Task_{task.Id}/source.txt", (await _context.TaskFiles.FindAsync(file.Id))!.StoredPath);
    }

    [Fact]
    public async Task AddFileAsync_RenamesDuplicateFile()
    {
        var task = await CreateTaskAsync("Задача");
        var firstSource = Path.Combine(_tempDirectory, "first", "report.pdf");
        var secondSource = Path.Combine(_tempDirectory, "second", "report.pdf");
        Directory.CreateDirectory(Path.GetDirectoryName(firstSource)!);
        Directory.CreateDirectory(Path.GetDirectoryName(secondSource)!);
        await File.WriteAllTextAsync(firstSource, "first");
        await File.WriteAllTextAsync(secondSource, "second");

        await _service.AddFileAsync(task.Id, firstSource);
        var renamed = await _service.AddFileAsync(task.Id, secondSource);

        Assert.Equal("report (1).pdf", renamed.FileName);
        Assert.True(File.Exists(Path.Combine(_tempDirectory, "TasksFiles", $"Task_{task.Id}", "report (1).pdf")));
    }

    [Fact]
    public async Task DeleteFileAsync_SoftDeletesRecord()
    {
        var task = await CreateTaskAsync("Задача");
        var sourcePath = Path.Combine(_tempDirectory, "keep.txt");
        await File.WriteAllTextAsync(sourcePath, "data");
        var file = await _service.AddFileAsync(task.Id, sourcePath);

        await _service.DeleteFileAsync(file.Id);

        var storedFile = await _context.TaskFiles.FindAsync(file.Id);
        Assert.NotNull(storedFile!.DeletedAtUtc);
        Assert.True(File.Exists(Path.Combine(_tempDirectory, "TasksFiles", $"Task_{task.Id}", "keep.txt")));
    }

    private async Task<TaskDb> CreateTaskAsync(string title)
    {
        var task = new TaskDb
        {
            Title = title,
            DueDateUtc = new DateTime(2026, 4, 1),
            ProgressPercent = 0,
            Priority = TaskPriority.Medium,
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        SqliteConnection.ClearAllPools();

        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);
    }

    private sealed class TestAppDataPathProvider(string root) : IAppDataPathProvider
    {
        public string GetAppDataDirectory() => root;

        public string GetDatabaseFilePath() => Path.Combine(root, "vt2.db");

        public string GetTaskFilesDirectory(int taskId) =>
            Path.Combine(root, "TasksFiles", $"Task_{taskId}");

        public string GetTaskFileStoredPath(int taskId, string fileName) =>
            $"TasksFiles/Task_{taskId}/{fileName}";
    }
}
