using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenantOrdersLab.Domain.Common;
using TenantOrdersLab.Domain.Entities;
using TenantOrdersLab.Domain.ValueObjects;
using Xunit;

namespace TenantOrdersLab.Domain.Tests.Order
{
    public class OrderAggregateTests
    {
        [Fact]
        public void Place_Should_Throw_When_Order_Is_Already_Placed()
        {
            // Arrange  
            var order = new TenantOrdersLab.Domain.Entities.Order(1, new Money(100, "USD"));
            order.Place();
            // Act & Assert  
            var exception = Assert.Throws<DomainException>(() => order.Place());
            Assert.Equal("Only new orders can be placed.", exception.Message);
        }

        [Fact]
        public void PullDomainEvents_Should_Clear_Events_After_Call()
        {
            // Arrange  
            var order = new TenantOrdersLab.Domain.Entities.Order(1, new Money(100, "USD"));
            order.Place();
            // Act  
            var events = order.PullDomainEvents();
            var eventsAfterPull = order.PullDomainEvents();
            // Assert  
            Assert.Single(events);
            Assert.Empty(eventsAfterPull);
        }
    }
}
