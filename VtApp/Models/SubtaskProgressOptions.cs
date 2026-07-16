namespace VtApp.Models;

public sealed class ProgressOption
{
    public required int Value { get; init; }

    public required string Display { get; init; }
}

public static class SubtaskProgressOptions
{
    public static IReadOnlyList<ProgressOption> All { get; } =
    [
        new() { Value = 0, Display = "0" },
        new() { Value = 33, Display = "1/3" },
        new() { Value = 67, Display = "2/3" },
        new() { Value = 100, Display = "V" },
    ];

    public static int Normalize(int progressPercent) =>
        progressPercent switch
        {
            <= 16 => 0,
            <= 50 => 33,
            <= 83 => 67,
            _ => 100,
        };
}
