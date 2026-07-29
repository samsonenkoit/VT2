using System.Text.Json;
using System.Text.RegularExpressions;
using System.IO;
using Installer.Models;

namespace Installer.Services;

public static partial class VersionParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [GeneratedRegex(@"^(?<major>\d+)_(?<minor>\d+)$", RegexOptions.CultureInvariant)]
    private static partial Regex FolderNameRegex();

    public static bool TryParseFolderName(string folderName, out AppVersion version)
    {
        version = new AppVersion();
        var match = FolderNameRegex().Match(folderName);
        if (!match.Success)
        {
            return false;
        }

        version = new AppVersion
        {
            Major = int.Parse(match.Groups["major"].Value),
            Minor = int.Parse(match.Groups["minor"].Value)
        };
        return true;
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
            return JsonSerializer.Deserialize<AppVersion>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    /// <summary>
    /// Extracts folder name from an S3 common prefix like "vt2/0_1/".
    /// </summary>
    public static bool TryParseCommonPrefix(string commonPrefix, string expectedRootPrefix, out AppVersion version)
    {
        version = new AppVersion();
        var normalized = commonPrefix.Trim('/');
        var root = expectedRootPrefix.Trim('/');

        string folderName;
        if (normalized.StartsWith(root + "/", StringComparison.Ordinal))
        {
            var relative = normalized[(root.Length + 1)..];
            var slash = relative.IndexOf('/');
            folderName = slash >= 0 ? relative[..slash] : relative;
        }
        else
        {
            // Prefix without root, e.g. "0_1"
            var slash = normalized.IndexOf('/');
            folderName = slash >= 0 ? normalized[..slash] : normalized;
        }

        return TryParseFolderName(folderName, out version);
    }
}
