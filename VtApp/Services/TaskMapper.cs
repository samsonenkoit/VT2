using Database.Models;
using VtApp.Models;

namespace VtApp.Services;

public static class TaskMapper
{
    public static TaskItem ToTaskItem(TaskDb db) => new()
    {
        Id = db.Id,
        Title = db.Title,
        DueDate = db.DueDate,
        ProgressPercent = db.ProgressPercent,
        Priority = db.Priority,
        EmailCount = 0,
        BadgeCounts = [],
    };
}
