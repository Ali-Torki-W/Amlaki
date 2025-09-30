namespace Domain.ValueObjects.User;

using System.Text.RegularExpressions;
using Domain.Primitives;

public sealed class Email : ValueObject
{
    public string Value { get; }
    private static readonly Regex Pattern =
        new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private Email(string value) => Value = value;

    public static Email Create(string email)
    {
        email = Guard.NotEmpty(email, nameof(email)).Trim().ToLowerInvariant();
        Guard.Range(Pattern.IsMatch(email), "Invalid email format.");
        return new Email(email);
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value;
}