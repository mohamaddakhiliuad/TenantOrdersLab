using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenantOrdersLab.Domain
{
    public class Money 
    {
        public decimal Amount { get; }
        public string Currency { get; }
        public Money(decimal amount, string currency)
        {
            if (amount < 0)
                 throw new ArgumentException("Amount cannot be negative", nameof(amount));
            if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency cannot be null or empty", nameof(currency));
            if (currency.Length != 3) throw new ArgumentException("Currency must be a 3-letter ISO code", nameof(currency));
             Amount = amount;
            Currency = currency;
        }

        public Money Add(Money other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (Currency != other.Currency) throw new InvalidOperationException("Cannot add amounts with different currencies");
            return new Money(Amount + other.Amount, Currency);
        }

        public static object Of(decimal totalAmount, string currency)
        {
            throw new NotImplementedException();
        }
    }
}
