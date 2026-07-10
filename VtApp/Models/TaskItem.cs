using Database.Models;

namespace VtApp.Models;

public class TaskItem
{
    public int Id { get; init; }

    public required string Title { get; init; }

    public DateTime DueDateUtc { get; init; }

    public int ProgressPercent { get; init; }

    public TaskPriority Priority { get; init; }

    public int EmailCount { get; init; }

    public IReadOnlyList<int> BadgeCounts { get; init; } = [];

    public string DateProgressText => $"{DueDateUtc:dd.MM.yyyy} - {ProgressPercent}%";
}
