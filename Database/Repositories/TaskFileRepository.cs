using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class TaskFileRepository(VtDbContext context) : ITaskFileRepository
{
    public async Task<IReadOnlyList<TaskFileDb>> GetNotDeletedAsync(
        int taskId,
        CancellationToken cancellationToken = default)
    {
        return await context.TaskFiles
            .Where(f => f.TaskId == taskId && f.DeletedAtUtc == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskFileDb> AddAsync(TaskFileDb file, CancellationToken cancellationToken = default)
    {
        context.TaskFiles.Add(file);
        await context.SaveChangesAsync(cancellationToken);
        return file;
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await context.TaskFiles.FindAsync([id], cancellationToken);
        if (existing is null || existing.DeletedAtUtc is not null)
            return;

        existing.DeletedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }
}
