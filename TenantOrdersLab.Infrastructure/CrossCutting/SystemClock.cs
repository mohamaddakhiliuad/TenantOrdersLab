using TenantOrdersLab.App.Abstractions.Common;

namespace TenantOrdersLab.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
