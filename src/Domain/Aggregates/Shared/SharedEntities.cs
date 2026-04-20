using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Shared; 

   

/// <summary>
/// Value object for monetary amounts
/// </summary>
public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";

    private Money() { }

    public Money(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        Amount = amount;
        Currency = currency;
    }

    public static Money Zero => new(0);
    public static Money operator +(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money operator *(Money a, decimal multiplier) =>
        new(a.Amount * multiplier, a.Currency);

    public static bool operator >(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return a.Amount > b.Amount;
    }

    public static bool operator <(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return a.Amount < b.Amount;
    }

    private static void EnsureSameCurrency(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException($"Cannot operate on different currencies: {a.Currency} and {b.Currency}");
    }

    public override string ToString() => $"{Currency} {Amount:N2}";
}
 

public record Percentage
{
    public decimal Value { get; init; }

    private Percentage() { }

    public Percentage(decimal value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentException("Percentage must be between 0 and 100");
        Value = value;
    }

    public decimal ToDecimal() => Value / 100;
    public static Percentage Zero => new(0);

    public Money ApplyTo(Money amount) =>
        new(amount.Amount * (1 - ToDecimal()), amount.Currency);
}
