using Database.Models;

namespace Database.Repositories;

public interface IGoalRepository
{
    Task<IReadOnlyList<GoalDb>> GetNotDeletedAsync(int taskId, CancellationToken cancellationToken = default);

    Task<GoalDb> AddAsync(GoalDb goal, CancellationToken cancellationToken = default);

    Task UpdateAsync(GoalDb goal, CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
