namespace Domain.ValueObjects.Property;

using Domain.Primitives;
using Domain.Enums.Property;

public sealed class Amenities : ValueObject
{
    public bool Elevator { get; }
    public bool Balcony { get; }
    public decimal? GardenSqm { get; }
    public HeatingSystem Heating { get; }
    public CoolingSystem Cooling { get; }
    public Furnishing Furnishing { get; }
    public IReadOnlyCollection<string> SpecialItems => _specialItems.AsReadOnly();
    private readonly List<string> _specialItems;

    public Amenities(
        bool elevator,
        bool balcony,
        decimal? gardenSqm,
        HeatingSystem heating,
        CoolingSystem cooling,
        Furnishing furnishing,
        IEnumerable<string>? specialItems = null)
    {
        Guard.Range(gardenSqm is null or >= 0, "Garden size cannot be negative.");
        Elevator = elevator; Balcony = balcony; GardenSqm = gardenSqm;
        Heating = heating; Cooling = cooling; Furnishing = furnishing;
        _specialItems = (specialItems ?? Array.Empty<string>())
                        .Select(s => s.Trim())
                        .Where(s => s.Length > 0)
                        .Distinct()
                        .ToList();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Elevator; yield return Balcony; yield return GardenSqm;
        yield return Heating; yield return Cooling; yield return Furnishing;
        foreach (var s in _specialItems) yield return s;
    }
}
