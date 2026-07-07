namespace Database;

public sealed class AppDataDatabasePathProvider : IDatabasePathProvider
{
    public string GetDatabaseFilePath()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VT2");

        return Path.Combine(directory, "vt2.db");
    }
}
