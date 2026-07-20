namespace Database.Models;

public class GoalDb
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public required string Text { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public TaskDb Task { get; set; } = null!;
}
