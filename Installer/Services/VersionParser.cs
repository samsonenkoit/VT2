using System.Text.Json;
using System.IO;
using Installer.Models;

namespace Installer.Services;

public static class VersionParser
{
    public static AppVersion ParseVersionJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException(
                "Файл version.json пуст или недоступен.");
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException(
                    "Файл version.json должен содержать JSON-объект с полями major и minor.");
            }

            if (!document.RootElement.TryGetProperty("major", out var majorElement) ||
                !document.RootElement.TryGetProperty("minor", out var minorElement))
            {
                throw new InvalidOperationException(
                    "Файл version.json должен содержать поля major и minor.");
            }

            if (majorElement.ValueKind != JsonValueKind.Number ||
                !majorElement.TryGetInt32(out var major) ||
                major < 0)
            {
                throw new InvalidOperationException(
                    "Поле major в version.json должно быть неотрицательным целым числом.");
            }

            if (minorElement.ValueKind != JsonValueKind.Number ||
                !minorElement.TryGetInt32(out var minor) ||
                minor < 0)
            {
                throw new InvalidOperationException(
                    "Поле minor в version.json должно быть неотрицательным целым числом.");
            }

            return new AppVersion
            {
                Major = major,
                Minor = minor
            };
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                "Не удалось разобрать version.json: некорректный JSON.", ex);
        }
    }

    public static AppVersion? TryParseVersionJson(string json)
    {
        try
        {
            return ParseVersionJson(json);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public static AppVersion? TryReadVersionJson(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(filePath);
            return TryParseVersionJson(json);
        }
        catch (IOException)
        {
            return null;
        }
    }
}
