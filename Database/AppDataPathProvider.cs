namespace Database;

public sealed class AppDataPathProvider : IAppDataPathProvider
{
    private const string TasksFilesFolderName = "TasksFiles";

    public string GetAppDataDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "iFellowTracker");
    }

    public string GetDatabaseFilePath()
    {
        return Path.Combine(GetAppDataDirectory(), "iFellowTracker.db");
    }

    public string GetSettingsFilePath()
    {
        return Path.Combine(GetAppDataDirectory(), "settings.json");
    }

    public string GetTaskFilesDirectory(int taskId)
    {
        return Path.Combine(GetAppDataDirectory(), TasksFilesFolderName, $"Task_{taskId}");
    }
}
