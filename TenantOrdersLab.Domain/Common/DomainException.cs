using System;

namespace TenantOrdersLab.Domain.Common
{
    /// <summary>
    /// Represents a domain-specific rule violation.
    /// Thrown when a business invariant is broken inside the domain.
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message)
            : base(message)
        {
        }

        public DomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

