namespace VtApp.Models;

public sealed class AppVersion
{
    public int Major { get; set; } = 1;

    public int Minor { get; set; }

    public string Display => $"{Major}.{Minor}";
}
