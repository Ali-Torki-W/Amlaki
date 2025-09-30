namespace Domain.ValueObjects.Property;

using Domain.Primitives;

public sealed class Interior : ValueObject
{
    public int Bedrooms { get; }
    public int Bathrooms { get; }
    public int Parking { get; }

    public Interior(int bedrooms, int bathrooms, int parking)
    {
        Guard.Range(bedrooms >= 0 && bathrooms >= 0 && parking >= 0, "Counts cannot be negative.");
        Bedrooms = bedrooms; Bathrooms = bathrooms; Parking = parking;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return Bedrooms; yield return Bathrooms; yield return Parking; }
}
