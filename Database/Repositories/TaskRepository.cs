using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class TaskRepository(VtDbContext context) : ITaskRepository
{
    public async Task<IReadOnlyList<TaskDb>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await context.Tasks
            .Where(t => t.DeletedAt == null)
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskDb?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Tasks.FindAsync([id], cancellationToken);
    }

    public async Task<TaskDb> AddAsync(TaskDb task, CancellationToken cancellationToken = default)
    {
        context.Tasks.Add(task);
        await context.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task UpdateAsync(TaskDb task, CancellationToken cancellationToken = default)
    {
        context.Tasks.Update(task);
        await context.SaveChangesAsync(cancellationToken);
    }
}
