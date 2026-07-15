namespace VtApp.Models;

public sealed class EnumOption<T> where T : struct, Enum
{
    public required T Value { get; init; }

    public required string Display { get; init; }
}
