namespace TenantOrdersLab.Domain.Abstractions
{
    // Marker: یعنی این Entity باید Tenant داشته باشد
    public interface ITenantScoped { }

    // Marker: یعنی این Entity باید Created/Updated داشته باشد
    public interface IAudited { }
}
