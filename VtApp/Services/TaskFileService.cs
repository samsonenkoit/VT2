using System.IO;
using Database;
using VtApp.Models;

namespace VtApp.Services;

public sealed class TaskFileService(IAppDataPathProvider pathProvider) : ITaskFileService
{
    public Task<IReadOnlyList<TaskFileItem>> GetFilesAsync(
        int taskId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var taskDirectory = pathProvider.GetTaskFilesDirectory(taskId);
        if (!Directory.Exists(taskDirectory))
            return Task.FromResult<IReadOnlyList<TaskFileItem>>([]);

        var files = Directory.GetFiles(taskDirectory)
            .Select(path => new TaskFileItem { FileName = Path.GetFileName(path) })
            .OrderBy(f => f.FileName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult<IReadOnlyList<TaskFileItem>>(files);
    }

    public Task<TaskFileItem> AddFileAsync(
        int taskId,
        string sourceFilePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(sourceFilePath))
            throw new FileNotFoundException("Файл не найден.", sourceFilePath);

        var sourceFileName = Path.GetFileName(sourceFilePath);
        var taskDirectory = pathProvider.GetTaskFilesDirectory(taskId);
        Directory.CreateDirectory(taskDirectory);

        var existingNames = GetExistingFileNames(taskDirectory);
        var uniqueFileName = GetUniqueFileName(sourceFileName, existingNames);
        var destinationPath = Path.Combine(taskDirectory, uniqueFileName);

        File.Move(sourceFilePath, destinationPath);

        return Task.FromResult(new TaskFileItem { FileName = uniqueFileName });
    }

    public Task DeleteFileAsync(
        int taskId,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(fileName))
            return Task.CompletedTask;

        var filePath = Path.Combine(pathProvider.GetTaskFilesDirectory(taskId), fileName);
        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    private static HashSet<string> GetExistingFileNames(string taskDirectory)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(taskDirectory))
            return names;

        foreach (var filePath in Directory.GetFiles(taskDirectory))
            names.Add(Path.GetFileName(filePath));

        return names;
    }

    internal static string GetUniqueFileName(string desiredName, IEnumerable<string> existingNames)
    {
        var existing = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);
        if (!existing.Contains(desiredName))
            return desiredName;

        var baseName = Path.GetFileNameWithoutExtension(desiredName);
        var extension = Path.GetExtension(desiredName);

        for (var index = 1; ; index++)
        {
            var candidate = $"{baseName} ({index}){extension}";
            if (!existing.Contains(candidate))
                return candidate;
        }
    }
}
