using VtApp.Models;

namespace VtApp.Services;

public sealed class AppVersionService : IAppVersionService
{
    public AppVersion Current { get; } = new()
    {
        Major = 0,
        Minor = 1
    };
}
