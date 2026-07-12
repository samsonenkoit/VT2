namespace Database;

public interface IAppDataPathProvider
{
    string GetAppDataDirectory();

    string GetDatabaseFilePath();

    string GetTaskFilesDirectory(int taskId);

    string GetTaskFileStoredPath(int taskId, string fileName);
}
