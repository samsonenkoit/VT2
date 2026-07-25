namespace Database;

public interface IAppDataPathProvider
{
    string GetAppDataDirectory();

    string GetDatabaseFilePath();

    string GetSettingsFilePath();

    string GetTaskFilesDirectory(int taskId);
}
