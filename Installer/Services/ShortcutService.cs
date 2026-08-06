using System.IO;
using System.Runtime.InteropServices;

namespace Installer.Services;

public sealed class ShortcutService
{
    public string DesktopShortcutPath { get; }

    public ShortcutService()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            "iFellowTracker.lnk"))
    {
    }

    public ShortcutService(string desktopShortcutPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(desktopShortcutPath);
        DesktopShortcutPath = desktopShortcutPath;
    }

    public void CreateOrUpdateDesktopShortcut(string targetExePath, string workingDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetExePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);

        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException("Не удалось создать ярлык: WScript.Shell недоступен.");

        dynamic shell = Activator.CreateInstance(shellType)
            ?? throw new InvalidOperationException("Не удалось создать экземпляр WScript.Shell.");

        try
        {
            var shortcut = shell.CreateShortcut(DesktopShortcutPath);
            shortcut.TargetPath = targetExePath;
            shortcut.WorkingDirectory = workingDirectory;
            shortcut.Description = "Айфэллоу Трекер";
            shortcut.IconLocation = targetExePath + ",0";
            shortcut.Save();
        }
        finally
        {
            if (Marshal.IsComObject(shell))
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }
    }
}
