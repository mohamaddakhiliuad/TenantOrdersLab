using System.Threading;
using System.Threading.Tasks;
using TenantOrdersLab.Domain.Entities;

namespace TenantOrdersLab.App.Abstractions.Persistence
{
    /// <summary>
    /// Repository for the Customer aggregate root.
    ///
    /// Purpose:
    /// - Load customer aggregates for validation / write-side operations
    /// - Add new customer aggregate when needed
    ///
    /// Notes:
    /// - Tenant enforcement is an Infrastructure concern (query filters / scoping).
    /// </summary>
    public interface ICustomerRepository
    {
        /// <summary>
        /// Loads a Customer aggregate intended for modification in a write use case.
        /// </summary>
        Task<Customer?> GetForUpdateAsync(int customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new Customer aggregate to the persistence session.
        /// </summary>
        void Add(Customer customer);
    }
}
