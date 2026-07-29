namespace Installer.Models;

public sealed class AppVersion : IComparable<AppVersion>
{
    public int Major { get; set; }

    public int Minor { get; set; } = 1;

    public string Display => $"{Major}.{Minor}";

    public string FolderName => $"{Major}_{Minor}";

    public int CompareTo(AppVersion? other)
    {
        if (other is null)
        {
            return 1;
        }

        var majorCompare = Major.CompareTo(other.Major);
        return majorCompare != 0 ? majorCompare : Minor.CompareTo(other.Minor);
    }

    public bool IsNewerThan(AppVersion other) => CompareTo(other) > 0;
}
