using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TenantOrdersLab.Domain;

namespace TenantOrdersLab.App.Abstractions
{
    /// <summary>
    /// Minimal persistence boundary for the Application layer.
    ///
    /// Purpose:
    /// - Expose aggregate roots required by use cases (Orders, Customers)
    /// - Provide a commit mechanism (SaveChanges)
    /// 
    /// What this interface MUST NOT contain:
    /// - Business behavior (PlaceOrder, Pay, Cancel, etc.)
    /// - EF Core specific APIs (DbContext, ChangeTracker, Include, etc.)
    ///
    /// The Application layer orchestrates behavior.
    /// The Infrastructure layer persists state.
    /// </summary>
    public interface IOrdersDbContext
    {
        /// <summary>
        /// Orders aggregate root.
        /// Used by:
        /// - Write side (tracked loading for commands)
        /// - Read side (projection + NoTracking in Infrastructure)
        /// </summary>
        IQueryable<Order> Orders { get; }

        /// <summary>
        /// Customers aggregate root.
        /// Used for:
        /// - Validation (customer exists)
        /// - Creating new orders for a customer
        /// </summary>
        IQueryable<Customer> Customers { get; }

        /// <summary>
        /// Commits all changes made during the current use case.
        /// Acts as the transaction boundary for the Application layer.
        ///
        /// Domain events (if any) are collected AFTER a successful commit.
        /// </summary>
        /// 
          // --- Write-side retrieval (tracked) ---
        Task<Customer?> GetCustomerForUpdateAsync(int customerId, CancellationToken cancellationToken = default);
        Task<Order?> GetOrderForUpdateAsync(int orderId, CancellationToken cancellationToken = default);

        // --- Write-side persistence ---
        void AddCustomer(Customer customer);
        void AddOrder(Order order);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
