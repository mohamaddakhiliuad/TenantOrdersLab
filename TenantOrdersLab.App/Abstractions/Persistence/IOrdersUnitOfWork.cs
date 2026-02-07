using System.Threading;
using System.Threading.Tasks;

namespace TenantOrdersLab.App.Abstractions.Persistence
{
    /// <summary>
    /// Unit of Work boundary for the Application layer.
    ///
    /// Responsibilities:
    /// - Expose repositories required by use cases
    /// - Provide a single commit point (transaction boundary)
    ///
    /// What it MUST NOT contain:
    /// - Business behaviors (PlaceOrder/Pay/Cancel/etc.)
    /// - EF Core APIs (DbContext, ChangeTracker, Include, etc.)
    /// </summary>
    public interface IOrdersUnitOfWork
    {
        IOrderRepository Orders { get; }
        ICustomerRepository Customers { get; }

        /// <summary>
        /// Commits all changes performed in the current use case.
        /// This is the transaction boundary from the Application perspective.
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
