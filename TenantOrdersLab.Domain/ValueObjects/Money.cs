using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenantOrdersLab.Domain.ValueObjects
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

        public static Money Of(decimal totalAmount, string currency)
        {
            if (totalAmount < 0)
                throw new ArgumentOutOfRangeException(nameof(totalAmount), "Amount cannot be negative.");

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency is required.", nameof(currency));

            // Normalize currency (simple ISO-ish guard)
            currency = currency.Trim().ToUpperInvariant();

            if (currency.Length != 3)
                throw new ArgumentException("Currency must be a 3-letter code (e.g., USD).", nameof(currency));

            // Optional: round to 2 decimals (depends on your domain rules)
            totalAmount = decimal.Round(totalAmount, 2, MidpointRounding.AwayFromZero);

            return new Money(totalAmount, currency);
        }
    }
}
