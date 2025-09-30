namespace Domain.ValueObjects.User;

using System.Text.RegularExpressions;
using Domain.Primitives;

public sealed class PhoneNumber : ValueObject
{
    public string E164 { get; } // e.g., +989121234567
    private static readonly Regex E164Pattern =
        new(@"^\+[1-9]\d{6,14}$", RegexOptions.Compiled); // ITU E.164 length 7..15

    private PhoneNumber(string e164) => E164 = e164;

    public static PhoneNumber Create(string raw)
    {
        raw = Guard.NotEmpty(raw, nameof(raw)).Replace(" ", "").Replace("-", "");
        Guard.Range(E164Pattern.IsMatch(raw), "Phone must be E.164 (e.g., +98912XXXXXXX).");
        return new PhoneNumber(raw);
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return E164; }
    public override string ToString() => E164;
}
