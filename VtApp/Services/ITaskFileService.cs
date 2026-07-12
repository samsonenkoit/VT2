using VtApp.Models;

namespace VtApp.Services;

public interface ITaskFileService
{
    Task<IReadOnlyList<TaskFileItem>> GetFilesAsync(int taskId, CancellationToken cancellationToken = default);

    Task<TaskFileItem> AddFileAsync(int taskId, string sourceFilePath, CancellationToken cancellationToken = default);

    Task DeleteFileAsync(int fileId, CancellationToken cancellationToken = default);
}
