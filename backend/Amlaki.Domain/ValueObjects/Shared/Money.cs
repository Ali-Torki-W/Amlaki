namespace Domain.ValueObjects.Shared;

using Domain.Primitives;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "IRR")
    {
        Guard.Range(!string.IsNullOrWhiteSpace(currency), "Currency is required.");
        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
    }

    public bool IsNegative => Amount < 0m;

    public Money WithAmount(decimal newAmount) => new(newAmount, Currency);

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return Amount; yield return Currency; }

    public override string ToString() => $"{Amount:0.##} {Currency}";
}
