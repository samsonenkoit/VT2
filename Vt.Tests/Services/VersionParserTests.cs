using Installer.Services;
using Xunit;

namespace Vt.Tests.Services;

public class VersionParserTests
{
    [Fact]
    public void ParseVersionJson_WhenValid_ReturnsVersion()
    {
        var version = VersionParser.ParseVersionJson("""{"major": 0, "minor": 1}""");

        Assert.Equal(0, version.Major);
        Assert.Equal(1, version.Minor);
    }

    [Fact]
    public void ParseVersionJson_WhenMissingFields_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => VersionParser.ParseVersionJson("""{"major": 1}"""));

        Assert.Contains("major и minor", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ParseVersionJson_WhenInvalidJson_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => VersionParser.ParseVersionJson("{ broken"));

        Assert.Contains("некорректный JSON", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TryParseVersionJson_WhenInvalid_ReturnsNull()
    {
        Assert.Null(VersionParser.TryParseVersionJson("""{"major": -1, "minor": 1}"""));
    }

    [Fact]
    public void TryReadVersionJson_WhenFileMissing_ReturnsNull()
    {
        var path = Path.Combine(Path.GetTempPath(), "VT2_missing_version_" + Guid.NewGuid().ToString("N") + ".json");

        Assert.Null(VersionParser.TryReadVersionJson(path));
    }
}
