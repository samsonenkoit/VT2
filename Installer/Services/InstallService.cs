using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Installer.Models;

namespace Installer.Services;

public sealed class InstallService
{
    public const string ExeFileName = "VtApp.exe";
    public const string VersionFileName = "version.json";

    public string InstallDirectory { get; }

    public string ExePath => Path.Combine(InstallDirectory, ExeFileName);

    public string VersionFilePath => Path.Combine(InstallDirectory, VersionFileName);

    public InstallService()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VT2",
            "App"))
    {
    }

    public InstallService(string installDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(installDirectory);
        InstallDirectory = installDirectory;
    }

    public bool IsInstalled() => File.Exists(ExePath);

    public AppVersion? GetInstalledVersion()
    {
        if (!IsInstalled())
        {
            return null;
        }

        return VersionParser.TryReadVersionJson(VersionFilePath);
    }

    public bool IsAppRunning()
    {
        try
        {
            return Process.GetProcessesByName("VtApp").Length > 0;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public void InstallFromZip(string zipPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(zipPath);
        if (!File.Exists(zipPath))
        {
            throw new FileNotFoundException("Архив приложения не найден.", zipPath);
        }

        var stagingRoot = Path.Combine(
            Path.GetTempPath(),
            "VT2_Install_" + Guid.NewGuid().ToString("N"));
        var stagingContent = Path.Combine(stagingRoot, "content");

        try
        {
            Directory.CreateDirectory(stagingContent);
            ZipFile.ExtractToDirectory(zipPath, stagingContent);

            if (!File.Exists(Path.Combine(stagingContent, ExeFileName)))
            {
                throw new InvalidOperationException(
                    $"В архиве не найден {ExeFileName}. Проверьте содержимое self-contained.zip.");
            }

            Directory.CreateDirectory(InstallDirectory);
            ClearDirectory(InstallDirectory);
            CopyDirectory(stagingContent, InstallDirectory);
        }
        finally
        {
            if (Directory.Exists(stagingRoot))
            {
                try
                {
                    Directory.Delete(stagingRoot, recursive: true);
                }
                catch (IOException)
                {
                    // Best-effort cleanup of temp staging.
                }
            }
        }
    }

    private static void ClearDirectory(string directory)
    {
        foreach (var file in Directory.EnumerateFiles(directory))
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (var subDir in Directory.EnumerateDirectories(directory))
        {
            Directory.Delete(subDir, recursive: true);
        }
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        foreach (var directory in Directory.EnumerateDirectories(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDir, directory);
            Directory.CreateDirectory(Path.Combine(destinationDir, relative));
        }

        foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDir, file);
            var dest = Path.Combine(destinationDir, relative);
            var destDir = Path.GetDirectoryName(dest);
            if (!string.IsNullOrEmpty(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            File.Copy(file, dest, overwrite: true);
        }
    }
}
