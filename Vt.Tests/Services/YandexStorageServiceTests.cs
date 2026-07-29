using System.Net;
using System.Net.Http;
using System.Text;
using Installer.Services;
using Xunit;

namespace Vt.Tests.Services;

public class YandexStorageServiceTests
{
    [Fact]
    public async Task GetRemoteVersionAsync_WhenJsonValid_ReturnsVersion()
    {
        using var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            """{"major": 2, "minor": 5}""");
        using var httpClient = new HttpClient(handler);
        using var service = new YandexStorageService(httpClient);

        var version = await service.GetRemoteVersionAsync();

        Assert.Equal(2, version.Major);
        Assert.Equal(5, version.Minor);
        Assert.Equal("2.5", version.Display);
        Assert.Equal("2_5", version.FolderName);
        Assert.Equal(YandexStorageService.VersionJsonUri, handler.LastRequestUri);
    }

    [Fact]
    public async Task GetRemoteVersionAsync_WhenHttpFails_ThrowsWithStatus()
    {
        using var handler = new StubHttpMessageHandler(HttpStatusCode.NotFound, "missing");
        using var httpClient = new HttpClient(handler);
        using var service = new YandexStorageService(httpClient);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetRemoteVersionAsync());

        Assert.Contains("version.json", ex.Message, StringComparison.Ordinal);
        Assert.Contains("404", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetRemoteVersionAsync_WhenJsonInvalid_Throws()
    {
        using var handler = new StubHttpMessageHandler(HttpStatusCode.OK, "{ not valid");
        using var httpClient = new HttpClient(handler);
        using var service = new YandexStorageService(httpClient);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetRemoteVersionAsync());

        Assert.Contains("version.json", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GetSelfContainedZipUrl_UsesVersionFolder()
    {
        using var service = new YandexStorageService(new HttpClient());

        var url = service.GetSelfContainedZipUrl(new Installer.Models.AppVersion { Major = 0, Minor = 1 });

        Assert.Equal(
            "https://storage.yandexcloud.net/vt2/vt2/0_1/self-contained.zip",
            url);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        public Uri? LastRequestUri { get; private set; }

        public StubHttpMessageHandler(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
