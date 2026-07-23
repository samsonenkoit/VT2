namespace Database;

public interface IAppDataPathProvider
{
    string GetAppDataDirectory();

    string GetDatabaseFilePath();

    string GetTaskFilesDirectory(int taskId);
}
