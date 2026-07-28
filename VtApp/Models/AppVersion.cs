namespace VtApp.Models;

public sealed class AppVersion
{
    public int Major { get; set; }

    public int Minor { get; set; } = 1;

    public string Display => $"{Major}.{Minor}";
}
