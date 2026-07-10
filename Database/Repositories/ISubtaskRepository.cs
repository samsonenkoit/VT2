using Database.Models;

namespace Database.Repositories;

public interface ISubtaskRepository
{
    Task<IReadOnlyList<SubtaskDb>> GetNotDeletedAsync(int taskId, CancellationToken cancellationToken = default);

    Task<SubtaskDb> AddAsync(SubtaskDb subtask, CancellationToken cancellationToken = default);

    Task UpdateAsync(SubtaskDb subtask, CancellationToken cancellationToken = default);
}
