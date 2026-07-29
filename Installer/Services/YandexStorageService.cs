using System.Net.Http;
using System.IO;
using System.Xml.Linq;
using Installer.Models;

namespace Installer.Services;

public sealed class YandexStorageService : IDisposable
{
    public const string BucketName = "vt2";
    public const string ObjectPrefix = "vt2/";
    public const string SelfContainedZipName = "self-contained.zip";

    private static readonly XNamespace S3 = "http://s3.amazonaws.com/doc/2006-03-01/";
    private static readonly Uri ListUri =
        new($"https://storage.yandexcloud.net/{BucketName}?list-type=2&prefix={Uri.EscapeDataString(ObjectPrefix)}&delimiter=/");

    private readonly HttpClient _httpClient;

    public YandexStorageService()
        : this(new HttpClient { Timeout = TimeSpan.FromMinutes(30) })
    {
    }

    public YandexStorageService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AppVersion> GetRemoteVersionAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(ListUri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var document = XDocument.Parse(xml);

        var versions = document
            .Descendants(S3 + "CommonPrefixes")
            .Elements(S3 + "Prefix")
            .Select(e => e.Value)
            .Select(prefix =>
            {
                if (VersionParser.TryParseCommonPrefix(prefix, ObjectPrefix, out var version))
                {
                    return version;
                }

                return null;
            })
            .Where(v => v is not null)
            .Cast<AppVersion>()
            .ToList();

        if (versions.Count == 0)
        {
            throw new InvalidOperationException(
                "В хранилище не найдена папка версии приложения. Проверьте доступность бакета.");
        }

        if (versions.Count > 1)
        {
            throw new InvalidOperationException(
                $"В хранилище найдено несколько папок версий ({versions.Count}). Ожидается одна.");
        }

        return versions[0];
    }

    public string GetSelfContainedZipUrl(AppVersion version) =>
        $"https://storage.yandexcloud.net/{BucketName}/{ObjectPrefix}{version.FolderName}/{SelfContainedZipName}";

    public async Task DownloadFileAsync(
        string url,
        string destinationPath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient
            .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var total = response.Content.Headers.ContentLength;
        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using var destination = new FileStream(
            destinationPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        var buffer = new byte[81920];
        long downloaded = 0;
        int read;
        while ((read = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false)) > 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
            downloaded += read;
            if (total is > 0)
            {
                progress?.Report((double)downloaded / total.Value);
            }
        }

        progress?.Report(1.0);
    }

    public void Dispose() => _httpClient.Dispose();
}
