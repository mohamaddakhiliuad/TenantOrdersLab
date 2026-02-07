namespace TenantOrdersLab.App.Abstractions.Common;

public interface ITenantProvider
{
    string TenantId { get; }
}
