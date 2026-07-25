using Database.Helpers;
using Xunit;

namespace Vt.Tests.Services;

public class TaskProgressCalculatorTests
{
    [Fact]
    public void TryAverage_Empty_ReturnsFalse()
    {
        var ok = TaskProgressCalculator.TryAverage([], out var result);

        Assert.False(ok);
        Assert.Equal(0, result);
    }

    [Fact]
    public void TryAverage_SingleValue_ReturnsThatValue()
    {
        var ok = TaskProgressCalculator.TryAverage([67], out var result);

        Assert.True(ok);
        Assert.Equal(67, result);
    }

    [Fact]
    public void TryAverage_ThreeValues_RoundsAwayFromZero()
    {
        var ok = TaskProgressCalculator.TryAverage([0, 33, 100], out var result);

        Assert.True(ok);
        Assert.Equal(44, result);
    }

    [Fact]
    public void TryAverage_ClampsToZeroHundred()
    {
        var okHigh = TaskProgressCalculator.TryAverage([150, 200], out var high);
        var okLow = TaskProgressCalculator.TryAverage([-10, -20], out var low);

        Assert.True(okHigh);
        Assert.Equal(100, high);
        Assert.True(okLow);
        Assert.Equal(0, low);
    }
}
