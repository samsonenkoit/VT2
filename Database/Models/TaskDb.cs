namespace Database.Models;

public class TaskDb
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public DateTime DueDate { get; set; }

    public int ProgressPercent { get; set; }

    public TaskPriority Priority { get; set; }

    public DateTime? DeletedAt { get; set; }
}
