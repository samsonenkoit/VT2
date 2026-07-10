namespace Database.Models;

public class TaskDb
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public DateTime DueDateUtc { get; set; }

    public int ProgressPercent { get; set; }

    public TaskPriority Priority { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public ICollection<SubtaskDb> Subtasks { get; set; } = [];
}
