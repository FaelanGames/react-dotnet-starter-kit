using StarterKit.Domain.Interfaces.System;

namespace StarterKit.Infrastructure.Security;

// TODO: Possible to be removed
public sealed class SystemClock : ISystemClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
