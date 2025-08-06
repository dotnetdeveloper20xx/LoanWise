using System;

namespace LoanWise.Domain.ValueObjects
{
    /// <summary>
    /// Represents a monetary value with an associated currency.
    /// Immutable and used across all financial fields in the domain.
    /// </summary>
    public sealed class Money : IEquatable<Money>
    {
        /// <summary>
        /// The numeric value of the money.
        /// </summary>
        public decimal Value { get; }

        /// <summary>
        /// The currency code (e.g., GBP, USD).
        /// </summary>
        public string Currency { get; }

        /// <summary>
        /// Creates a new instance of Money.
        /// </summary>
        /// <param name="value">Amount must be non-negative.</param>
        /// <param name="currency">Currency code, defaults to GBP.</param>
        public Money(decimal value, string currency = "GBP")
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Money value cannot be negative.");

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency must be provided.", nameof(currency));

            Value = value;
            Currency = currency.ToUpperInvariant();
        }

        /// <summary>
        /// Returns a Money object representing zero value.
        /// </summary>
        public static Money Zero(string currency = "GBP") => new(0, currency);

        /// <summary>
        /// Adds two Money values of the same currency.
        /// </summary>
        public Money Add(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(Value + other.Value, Currency);
        }

        /// <summary>
        /// Subtracts another Money from this one.
        /// </summary>
        public Money Subtract(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(Value - other.Value, Currency);
        }

        /// <summary>
        /// Ensures two Money objects share the same currency.
        /// </summary>
        private void EnsureSameCurrency(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("Cannot operate on Money values with different currencies.");
        }

        public override string ToString() => $"{Currency} {Value:N2}";

        public bool Equals(Money? other)
        {
            if (other is null) return false;
            return Value == other.Value && Currency == other.Currency;
        }

        public override bool Equals(object? obj) => Equals(obj as Money);

        public override int GetHashCode() => HashCode.Combine(Value, Currency);

        public static bool operator ==(Money left, Money right) => left.Equals(right);
        public static bool operator !=(Money left, Money right) => !left.Equals(right);
    }
}
