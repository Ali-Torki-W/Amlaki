namespace Domain.ValueObjects.User;

using Domain.Primitives;

public sealed class PersonName : ValueObject
{
    public string First { get; }
    public string Last { get; }
    public string? Middle { get; }

    public PersonName(string first, string last, string? middle = null)
    {
        First = Guard.NotEmpty(first, nameof(first));
        Last = Guard.NotEmpty(last, nameof(last));
        Middle = string.IsNullOrWhiteSpace(middle) ? null : middle.Trim();
    }

    public string Full => Middle is null ? $"{First} {Last}" : $"{First} {Middle} {Last}";
    protected override IEnumerable<object?> GetEqualityComponents() { yield return First; yield return Last; yield return Middle; }
    public override string ToString() => Full;
}
