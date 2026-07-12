using System.IO;
using Database;
using Database.Models;
using Database.Repositories;
using VtApp.Models;

namespace VtApp.Services;

public sealed class TaskFileService(
    ITaskFileRepository taskFileRepository,
    IAppDataPathProvider pathProvider) : ITaskFileService
{
    public async Task<IReadOnlyList<TaskFileItem>> GetFilesAsync(
        int taskId,
        CancellationToken cancellationToken = default)
    {
        var files = await taskFileRepository.GetNotDeletedAsync(taskId, cancellationToken);
        return files.Select(ToTaskFileItem).ToList();
    }

    public async Task<TaskFileItem> AddFileAsync(
        int taskId,
        string sourceFilePath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(sourceFilePath))
            throw new FileNotFoundException("Файл не найден.", sourceFilePath);

        var sourceFileName = Path.GetFileName(sourceFilePath);
        var taskDirectory = pathProvider.GetTaskFilesDirectory(taskId);
        Directory.CreateDirectory(taskDirectory);

        var existingNames = await GetExistingFileNamesAsync(taskId, taskDirectory, cancellationToken);
        var uniqueFileName = GetUniqueFileName(sourceFileName, existingNames);
        var destinationPath = Path.Combine(taskDirectory, uniqueFileName);

        File.Copy(sourceFilePath, destinationPath, overwrite: false);

        var file = await taskFileRepository.AddAsync(new TaskFileDb
        {
            TaskId = taskId,
            FileName = uniqueFileName,
            StoredPath = pathProvider.GetTaskFileStoredPath(taskId, uniqueFileName),
        }, cancellationToken);

        return ToTaskFileItem(file);
    }

    public Task DeleteFileAsync(int fileId, CancellationToken cancellationToken = default) =>
        taskFileRepository.SoftDeleteAsync(fileId, cancellationToken);

    private async Task<HashSet<string>> GetExistingFileNamesAsync(
        int taskId,
        string taskDirectory,
        CancellationToken cancellationToken)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (Directory.Exists(taskDirectory))
        {
            foreach (var filePath in Directory.GetFiles(taskDirectory))
                names.Add(Path.GetFileName(filePath));
        }

        var dbFiles = await taskFileRepository.GetNotDeletedAsync(taskId, cancellationToken);
        foreach (var file in dbFiles)
            names.Add(file.FileName);

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

    private static TaskFileItem ToTaskFileItem(TaskFileDb file) => new()
    {
        Id = file.Id,
        FileName = file.FileName,
    };
}
