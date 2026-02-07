using System;

namespace TenantOrdersLab.App.Abstractions.Common;

public interface IClock
{
    DateTime UtcNow { get; }
}
