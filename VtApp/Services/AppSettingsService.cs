using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Database;
using VtApp.Models;

namespace VtApp.Services;

public sealed class AppSettingsService(IAppDataPathProvider pathProvider) : IAppSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public AppSettings Current { get; private set; } = new();

    public AppSettings Load()
    {
        var path = pathProvider.GetSettingsFilePath();
        if (!File.Exists(path))
        {
            Current = new AppSettings();
            return Current;
        }

        try
        {
            var json = File.ReadAllText(path);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            Current = settings ?? new AppSettings();
        }
        catch (JsonException)
        {
            Current = new AppSettings();
        }
        catch (IOException)
        {
            Current = new AppSettings();
        }

        return Current;
    }

    public void Save(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var path = pathProvider.GetSettingsFilePath();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(path, json);
        Current = settings;
    }
}
