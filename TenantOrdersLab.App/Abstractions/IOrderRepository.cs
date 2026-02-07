using System.Threading;
using System.Threading.Tasks;
using TenantOrdersLab.Domain;

namespace TenantOrdersLab.App.Abstractions
{
    /// <summary>
    /// Repository for the Order aggregate root.
    ///
    /// Purpose:
    /// - Provide aggregate retrieval for write-side use cases (tracked behavior is an Infrastructure detail)
    /// - Provide add semantics for new aggregates
    ///
    /// Important:
    /// - No IQueryable exposure (prevents ORM/provider leakage into Application)
    /// - No business actions (Place/Cancel/etc. are domain behaviors)
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// Loads an Order aggregate intended for modification in a write use case.
        /// Infrastructure ensures tenant safety and the appropriate tracking behavior.
        /// </summary>
        Task<Order?> GetForUpdateAsync(int orderId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new Order aggregate to the persistence session.
        /// </summary>
        void Add(Order order);
    }
}
