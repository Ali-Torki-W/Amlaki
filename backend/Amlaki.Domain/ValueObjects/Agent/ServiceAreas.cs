namespace Domain.ValueObjects.Agent;

using Domain.Primitives;

public sealed class ServiceAreas : ValueObject
{
    private readonly List<string> _areas;
    public IReadOnlyCollection<string> Areas => _areas.AsReadOnly();

    public ServiceAreas(IEnumerable<string> areas)
    {
        _areas = (areas ?? Array.Empty<string>())
            .Select(a => Guard.NotEmpty(a, "Area").ToLowerInvariant())
            .Distinct()
            .ToList();
    }

    public bool Contains(string cityOrRegion) => _areas.Contains(cityOrRegion.Trim().ToLowerInvariant());
    public ServiceAreas Replace(IEnumerable<string> areas) => new(areas);

    protected override IEnumerable<object?> GetEqualityComponents()
    { foreach (var a in _areas) yield return a; }
}
