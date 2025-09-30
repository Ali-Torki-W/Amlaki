namespace Domain.ValueObjects.Shared;

using Domain.Primitives;

public sealed class Tag : ValueObject
{
    public string Value { get; }

    public Tag(string value)
    {
        Value = Guard.NotEmpty(value, nameof(value)).ToLowerInvariant();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return Value; }

    public override string ToString() => Value;
}
