using System.Globalization;
using VtApp.Converters;
using Xunit;

namespace Vt.Tests.Converters;

public class ProgressAtLeastConverterTests
{
    private readonly ProgressAtLeastConverter _converter = new();

    [Theory]
    [InlineData(33, "33", true)]
    [InlineData(100, "33", true)]
    [InlineData(32, "33", false)]
    [InlineData(0, "33", false)]
    [InlineData(100, "100", true)]
    [InlineData(99, "100", false)]
    public void Convert_ComparesProgressToThreshold(int progress, string threshold, bool expected)
    {
        var result = _converter.Convert(progress, typeof(bool), threshold, CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_NonIntValue_ReturnsFalse()
    {
        var result = _converter.Convert("67", typeof(bool), "33", CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }

    [Fact]
    public void Convert_InvalidParameter_ReturnsFalse()
    {
        var result = _converter.Convert(50, typeof(bool), "abc", CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }
}
