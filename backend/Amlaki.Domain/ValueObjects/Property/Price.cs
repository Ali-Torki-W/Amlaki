namespace Domain.ValueObjects.Property;

using Domain.Primitives;
using Domain.ValueObjects.Shared;

public sealed class Price : ValueObject
{
    public Money Value { get; }

    public Price(Money money)
    {
        Guard.Range(!money.IsNegative, "Price cannot be negative.");
        Value = money;
    }

    public Price ChangeAmount(decimal newAmount) => new(new Money(newAmount, Value.Currency));

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return Value; }
}
