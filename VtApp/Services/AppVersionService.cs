using System.IO;
using System.Text.Json;
using VtApp.Models;

namespace VtApp.Services;

public sealed class AppVersionService : IAppVersionService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _filePath;

    public AppVersionService()
        : this(Path.Combine(AppContext.BaseDirectory, "version.json"))
    {
    }

    public AppVersionService(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = filePath;
    }

    public AppVersion Current { get; private set; } = CreateDefault();

    public AppVersion Load()
    {
        if (!File.Exists(_filePath))
        {
            Current = CreateDefault();
            return Current;
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            var version = JsonSerializer.Deserialize<AppVersion>(json, JsonOptions);
            Current = version ?? CreateDefault();
        }
        catch (JsonException)
        {
            Current = CreateDefault();
        }
        catch (IOException)
        {
            Current = CreateDefault();
        }

        return Current;
    }

    private static AppVersion CreateDefault() => new() { Major = 0, Minor = 1 };
}
