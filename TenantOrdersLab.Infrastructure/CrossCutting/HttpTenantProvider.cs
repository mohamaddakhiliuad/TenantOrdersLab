using Microsoft.AspNetCore.Http;
using TenantOrdersLab.App.Abstractions.Common;

namespace TenantOrdersLab.Infrastructure;

public sealed class HttpTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string TenantId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;

            if (context is null)
                return "default"; // Fallback (e.g. during migrations or background tasks)

            // Header name can be standardized
            var tenantHeader = context.Request.Headers["X-TenantId"].ToString();

            return string.IsNullOrWhiteSpace(tenantHeader)
                ? "default"
                : tenantHeader;
        }
    }
}
