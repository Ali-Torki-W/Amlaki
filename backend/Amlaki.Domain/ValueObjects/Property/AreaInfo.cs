namespace Domain.ValueObjects.Property;

using Domain.Primitives;

public sealed class AreaInfo : ValueObject
{
    public decimal LandSqm { get; }
    public decimal BuiltSqm { get; }
    public int FloorNumber { get; }

    public AreaInfo(decimal landSqm, decimal builtSqm, int floorNumber)
    {
        Guard.Range(landSqm >= 0, "Land area cannot be negative.");
        Guard.Range(builtSqm > 0, "Built area must be positive.");
        LandSqm = landSqm; BuiltSqm = builtSqm; FloorNumber = floorNumber;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return LandSqm; yield return BuiltSqm; yield return FloorNumber; }
}
