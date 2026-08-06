using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Installer.Services;

namespace Installer.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly YandexStorageService _storage;
    private readonly InstallService _install;
    private readonly ShortcutService _shortcut;

    [ObservableProperty]
    private string _statusText = "Нажмите «Установить / Обновить», чтобы начать.";

    [ObservableProperty]
    private double _progressValue;

    [ObservableProperty]
    private bool _isProgressIndeterminate;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(InstallOrUpdateCommand))]
    private bool _isBusy;

    public MainViewModel()
        : this(new YandexStorageService(), new InstallService(), new ShortcutService())
    {
    }

    public MainViewModel(
        YandexStorageService storage,
        InstallService install,
        ShortcutService shortcut)
    {
        _storage = storage;
        _install = install;
        _shortcut = shortcut;
    }

    private bool CanInstallOrUpdate() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanInstallOrUpdate))]
    private async Task InstallOrUpdateAsync()
    {
        IsBusy = true;
        ProgressValue = 0;
        IsProgressIndeterminate = true;

        string? zipPath = null;

        try
        {
            StatusText = "Проверка доступной версии…";
            var remoteVersion = await _storage.GetRemoteVersionAsync().ConfigureAwait(true);
            StatusText = $"Доступна версия {remoteVersion.Display}.";

            var installed = _install.IsInstalled();
            var localVersion = _install.GetInstalledVersion();

            if (installed && localVersion is not null && !remoteVersion.IsNewerThan(localVersion))
            {
                StatusText = $"Уже установлена актуальная версия {localVersion.Display}.";
                IsProgressIndeterminate = false;
                ProgressValue = 1;
                return;
            }

            if (installed)
            {
                while (_install.IsAppRunning())
                {
                    var result = MessageBox.Show(
                        "Приложение Айфэллоу Трекер запущено.\n\nЗакройте его и нажмите «ОК», чтобы продолжить обновление, или «Отмена» для выхода.",
                        "Обновление Айфэллоу Трекер",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Cancel)
                    {
                        StatusText = "Обновление отменено.";
                        IsProgressIndeterminate = false;
                        return;
                    }
                }
            }

            var action = installed ? "Обновление" : "Установка";
            StatusText = $"{action}: скачивание версии {remoteVersion.Display}…";
            IsProgressIndeterminate = false;
            ProgressValue = 0;

            zipPath = Path.Combine(
                Path.GetTempPath(),
                $"iFellowTracker_{remoteVersion.FolderName}_{Guid.NewGuid():N}.zip");

            var progress = new Progress<double>(value =>
            {
                ProgressValue = value;
                StatusText = $"{action}: скачивание… {(int)(value * 100)}%";
            });

            var url = _storage.GetSelfContainedZipUrl(remoteVersion);
            await _storage.DownloadFileAsync(url, zipPath, progress).ConfigureAwait(true);

            StatusText = $"{action}: распаковка и установка файлов…";
            IsProgressIndeterminate = true;

            await Task.Run(() => _install.InstallFromZip(zipPath)).ConfigureAwait(true);

            StatusText = "Создание ярлыка на рабочем столе…";
            _shortcut.CreateOrUpdateDesktopShortcut(_install.ExePath, _install.InstallDirectory);

            IsProgressIndeterminate = false;
            ProgressValue = 1;
            StatusText = installed
                ? $"Обновление до версии {remoteVersion.Display} завершено."
                : $"Установка версии {remoteVersion.Display} завершена.";
        }
        catch (Exception ex)
        {
            IsProgressIndeterminate = false;
            StatusText = "Ошибка: " + ex.Message;
            MessageBox.Show(
                ex.Message,
                "Ошибка установки",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            if (zipPath is not null && File.Exists(zipPath))
            {
                try
                {
                    File.Delete(zipPath);
                }
                catch (IOException)
                {
                    // Best-effort cleanup.
                }
            }

            IsBusy = false;
        }
    }
}
