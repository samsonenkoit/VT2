using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class SubtaskRepository(VtDbContext context) : ISubtaskRepository
{
    public async Task<IReadOnlyList<SubtaskDb>> GetNotDeletedAsync(
        int taskId,
        CancellationToken cancellationToken = default)
    {
        return await context.Subtasks
            .Where(s => s.TaskId == taskId && s.DeletedAtUtc == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<SubtaskDb> AddAsync(SubtaskDb subtask, CancellationToken cancellationToken = default)
    {
        context.Subtasks.Add(subtask);
        await context.SaveChangesAsync(cancellationToken);
        return subtask;
    }

    public async Task UpdateAsync(SubtaskDb subtask, CancellationToken cancellationToken = default)
    {
        var existing = await context.Subtasks.FindAsync([subtask.Id], cancellationToken);
        if (existing is null)
            return;

        existing.Title = subtask.Title;
        existing.TaskId = subtask.TaskId;
        await context.SaveChangesAsync(cancellationToken);
    }
}
