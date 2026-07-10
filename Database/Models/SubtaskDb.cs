namespace Database.Models;

public class SubtaskDb
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public int TaskId { get; set; }

    public TaskDb Task { get; set; } = null!;

    public DateTime? DeletedAt { get; set; }
}
