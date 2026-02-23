using TenantOrdersLab.Domain.Abstractions;
using TenantOrdersLab.Domain.Common;
using TenantOrdersLab.Domain.Events;
using TenantOrdersLab.Domain.ValueObjects;

namespace TenantOrdersLab.Domain.Entities
{

    public enum OrderStatus
    {
        New = 0,
        Placed = 1,
        Paid = 2,
        Canceld = 3,
        Completed = 4
    }
    public class Order : ITenantScoped, IAudited, IHasDomainEvents
    {
        private readonly List<IDomainEvent> _domainEvents = new();
        public int Id { get; private set; }
        public Money Total { get; private set; }
        public int CustomerId { get; }
        public Customer? Customer { get; private set; }
        public byte[] RowVersion { get; private set; } = default!;

        private Order() { } // For EF Core

        public Order(int customerId, Money total)
        {

                      
            if (customerId <= 0) throw new DomainException("validation: CustomerId must be a positive number.");
            // if (totalAmount <= 0) throw new DomainException("TotalAmount must be greater than zero.");
           // Id = id;
            Total = total ?? throw new ArgumentNullException(nameof(total));
            CustomerId = customerId;
            Status = OrderStatus.New;
        }
        public OrderStatus Status { get; private set; }

        // public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        public IReadOnlyCollection<IDomainEvent> PullDomainEvents()
        {
            var events = _domainEvents.ToList().AsReadOnly();
            _domainEvents.Clear();
            return events;
        }
        public void Place()
        {
            if (Status != OrderStatus.New)
                throw new DomainException("validation: Only new orders can be placed.");
            Status = OrderStatus.Placed;

            Raise(new OrderPlaced(Id, CustomerId));
        }

        private void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

        public static Order CreateNew(int customerID, Money total)
        {
            Order order = new Order(customerID, (Money)total); // CustomerId will be set later
          order.Raise(new OrderCreated(order.Id));
            return order;
        }

        public void Cancel(string reason)
        {
            //idempotency
            if (Status == OrderStatus.Canceld)
                return;
            
            if (Status != OrderStatus.Placed)
                throw new DomainException("validation: Only placed orders can be canceled.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("validation: Cancel reason is required.");

            Status = OrderStatus.Canceld;

            Raise(new OrderCanceled(Id, reason));
        }

        public void Complete()
        {
            if (Status == OrderStatus.Completed)
                return;
                           
       if (Status != OrderStatus.Paid)
                throw new DomainException("validation: Only Paid orders can be completed.");
            Status = OrderStatus.Completed;
            Raise(new OrderCompleted(Id));
        }
    }
}
