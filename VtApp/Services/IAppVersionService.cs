using VtApp.Models;

namespace VtApp.Services;

public interface IAppVersionService
{
    AppVersion Current { get; }

    AppVersion Load();
}
