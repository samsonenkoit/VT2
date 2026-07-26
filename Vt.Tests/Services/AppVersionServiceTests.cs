using VtApp.Services;
using Xunit;

namespace Vt.Tests.Services;

public class AppVersionServiceTests
{
    [Fact]
    public void Current_ReturnsCodedVersion()
    {
        var service = new AppVersionService();

        Assert.Equal(1, service.Current.Major);
        Assert.Equal(0, service.Current.Minor);
        Assert.Equal("1.0", service.Current.Display);
    }
}
