using System.Net.Http;
using System.IO;
using Installer.Models;

namespace Installer.Services;

public sealed class YandexStorageService : IDisposable
{
    public const string BucketName = "vt2";
    public const string ObjectPrefix = "vt2/";
    public const string VersionFileName = "version.json";
    public const string SelfContainedZipName = "self-contained.zip";

    public static readonly Uri VersionJsonUri =
        new($"https://storage.yandexcloud.net/{BucketName}/{VersionFileName}");

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
        using var response = await _httpClient
            .GetAsync(VersionJsonUri, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Не удалось загрузить {VersionFileName} из хранилища " +
                $"(HTTP {(int)response.StatusCode} {response.ReasonPhrase}).");
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return VersionParser.ParseVersionJson(json);
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
