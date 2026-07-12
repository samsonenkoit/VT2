namespace Database.Models;

public class TaskFileDb
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public TaskDb Task { get; set; } = null!;

    public required string FileName { get; set; }

    public required string StoredPath { get; set; }

    public DateTime? DeletedAtUtc { get; set; }
}
