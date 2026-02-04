
using TenantOrdersLab.Domain.Abstractions;
using TenantOrdersLab.Domain.Common;
using TenantOrdersLab.Domain.Events;

namespace TenantOrdersLab.Domain
{

    public enum OrderStatus
    {
        New = 0,
        Placed = 1,
        Paid = 2
    }
    public class Order: ITenantScoped, IAudited, IHasDomainEvents
    {
        private readonly List<IDomainEvent> _domainEvents = new();
        public int Id { get;  }
        public Money Total { get; private set; }
        public int CustomerId { get; }
        public Customer? Customer { get; private set; }

        private Order() { } // For EF Core

        public Order(int id, Money total, int customerId)
        {


            if (id <= 0) throw new DomainException("Id must be a positive number.");
            if (customerId <= 0) throw new DomainException("CustomerId must be a positive number.");
           // if (totalAmount <= 0) throw new DomainException("TotalAmount must be greater than zero.");
            Id = id;
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
                throw new DomainException("Only new orders can be placed.");
            Status = OrderStatus.Placed;

           Raise(new OrderPlaced(Id, CustomerId));
        }
       
        private void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    }
}
