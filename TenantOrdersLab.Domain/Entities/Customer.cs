using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenantOrdersLab.Domain.Abstractions;
using TenantOrdersLab.Domain.Common;

namespace TenantOrdersLab.Domain.Entities
{
    public sealed class Customer : ITenantScoped, IAudited
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public Customer(int id, string name)
        {
            if (id <= 0) throw new DomainException("Id must be positive.");
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required.");

            Id = id;
            Name = name;
        }
    }

}
