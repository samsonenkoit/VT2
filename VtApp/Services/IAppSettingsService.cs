using VtApp.Models;

namespace VtApp.Services;

public interface IAppSettingsService
{
    AppSettings Current { get; }

    AppSettings Load();

    void Save(AppSettings settings);
}
