namespace Domain.ValueObjects.Property;

using Domain.Primitives;

public sealed class Address : ValueObject
{
    public string City { get; }
    public string Neighborhood { get; }
    public string Line { get; }
    public string PostalCode { get; }
    public GeoLocation Location { get; }

    public Address(string city, string neighborhood, string line, string postalCode, GeoLocation location)
    {
        City = Guard.NotEmpty(city, nameof(city));
        Neighborhood = Guard.NotEmpty(neighborhood, nameof(neighborhood));
        Line = Guard.NotEmpty(line, nameof(line));
        PostalCode = Guard.NotEmpty(postalCode, nameof(postalCode));
        Location = location;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return City; yield return Neighborhood; yield return Line; yield return PostalCode; yield return Location; }
}
