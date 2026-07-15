namespace Database.Models;

public class TaskDb
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime DueDateUtc { get; set; }

    public int ProgressPercent { get; set; }

    public TaskPriority Priority { get; set; }

    public TaskImportance Importance { get; set; }

    public TaskDelayRisk DelayRisk { get; set; }

    public TaskDifficulty Difficulty { get; set; }

    public TaskUrgency Urgency { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public ICollection<SubtaskDb> Subtasks { get; set; } = [];
}
