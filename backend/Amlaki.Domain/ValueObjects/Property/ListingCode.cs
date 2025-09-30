namespace Domain.ValueObjects.Property;

using Domain.Primitives;

public sealed class ListingCode : ValueObject
{
    public string Value { get; }

    private ListingCode(string value) => Value = value;

    public static ListingCode Create(string code)
    {
        code = Guard.NotEmpty(code, nameof(code)).ToUpperInvariant();
        Guard.Range(code.Length is >= 3 and <= 16, "Listing code length must be 3–16.");
        Guard.Range(code.All(c => char.IsLetterOrDigit(c) || c is '-' or '_'), "Listing code contains invalid chars.");
        return new ListingCode(code);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return Value; }

    public override string ToString() => Value;
}
