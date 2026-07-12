using Database.Models;

namespace Database.Repositories;

public interface ITaskFileRepository
{
    Task<IReadOnlyList<TaskFileDb>> GetNotDeletedAsync(int taskId, CancellationToken cancellationToken = default);

    Task<TaskFileDb> AddAsync(TaskFileDb file, CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
