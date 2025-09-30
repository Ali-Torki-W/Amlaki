namespace Domain.ValueObjects.Property;

using Domain.Primitives;
using Domain.ValueObjects.Shared;

public sealed class Commission : ValueObject
{
    public Money Amount { get; }

    public Commission(Money amount)
    {
        Guard.Range(!amount.IsNegative, "Commission cannot be negative.");
        Amount = amount;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return Amount; }
}
