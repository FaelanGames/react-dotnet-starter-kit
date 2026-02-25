using StarterKit.Infrastructure.Security;

namespace StarterKit.Api.Tests;

public sealed class SystemClockTests
{
    [Fact]
    public void UtcNow_ReturnsCurrentUtcTime()
    {
        var clock = new SystemClock();
        var before = DateTime.UtcNow;

        var now = clock.UtcNow;

        var after = DateTime.UtcNow;
        Assert.InRange(now, before.AddSeconds(-1), after.AddSeconds(1));
    }
}
