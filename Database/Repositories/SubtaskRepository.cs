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

        existing.TaskId = subtask.TaskId;
        existing.Description = subtask.Description;
        existing.DueDateUtc = subtask.DueDateUtc;
        existing.ProgressPercent = subtask.ProgressPercent;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await context.Subtasks.FindAsync(id, cancellationToken);
        if (existing is null || existing.DeletedAtUtc is not null)
            return;

        existing.DeletedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }
}
