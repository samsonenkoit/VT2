using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class GoalRepository(VtDbContext context) : IGoalRepository
{
    public async Task<IReadOnlyList<GoalDb>> GetNotDeletedAsync(
        int taskId,
        CancellationToken cancellationToken = default)
    {
        return await context.Goals
            .Where(g => g.TaskId == taskId && g.DeletedAtUtc == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<GoalDb> AddAsync(GoalDb goal, CancellationToken cancellationToken = default)
    {
        context.Goals.Add(goal);
        await context.SaveChangesAsync(cancellationToken);
        return goal;
    }

    public async Task UpdateAsync(GoalDb goal, CancellationToken cancellationToken = default)
    {
        var existing = await context.Goals.FindAsync([goal.Id], cancellationToken);
        if (existing is null || existing.DeletedAtUtc is not null)
            return;

        existing.TaskId = goal.TaskId;
        existing.Text = goal.Text;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await context.Goals.FindAsync([id], cancellationToken);
        if (existing is null || existing.DeletedAtUtc is not null)
            return;

        existing.DeletedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }
}
