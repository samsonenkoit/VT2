namespace Database;

public sealed class AppDataPathProvider : IAppDataPathProvider
{
    private const string TasksFilesFolderName = "TasksFiles";

    public string GetAppDataDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VT2");
    }

    public string GetDatabaseFilePath()
    {
        return Path.Combine(GetAppDataDirectory(), "vt2.db");
    }

    public string GetTaskFilesDirectory(int taskId)
    {
        return Path.Combine(GetAppDataDirectory(), TasksFilesFolderName, $"Task_{taskId}");
    }
}
