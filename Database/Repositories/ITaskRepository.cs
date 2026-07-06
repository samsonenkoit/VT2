using Database.Models;

namespace Database.Repositories;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskDb>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    Task UpdateAsync(TaskDb task, CancellationToken cancellationToken = default);
}
