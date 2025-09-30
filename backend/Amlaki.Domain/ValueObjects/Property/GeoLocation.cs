namespace Domain.ValueObjects.Property;

using Domain.Primitives;

public readonly struct GeoLocation
{
    public double Latitude { get; }
    public double Longitude { get; }

    public GeoLocation(double latitude, double longitude)
    {
        Guard.Range(latitude is >= -90 and <= 90, "Latitude out of range.");
        Guard.Range(longitude is >= -180 and <= 180, "Longitude out of range.");
        Latitude = latitude; Longitude = longitude;
    }

    public override string ToString() => $"{Latitude},{Longitude}";
}
