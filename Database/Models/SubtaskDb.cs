namespace Database.Models;

public class SubtaskDb
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public int TaskId { get; set; }

    public string? Description { get; set; }

    public DateTime? DueDateUtc { get; set; }

    public int ProgressPercent { get; set; }

    public TaskDb Task { get; set; } = null!;

    public DateTime? DeletedAtUtc { get; set; }
}
