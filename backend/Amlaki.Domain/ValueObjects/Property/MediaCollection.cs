namespace Domain.ValueObjects.Property;

using Domain.Primitives;

public sealed class MediaCollection : ValueObject
{
    private readonly List<MediaItem> _items;
    public IReadOnlyList<MediaItem> Items => _items.AsReadOnly();

    public MediaCollection(IEnumerable<MediaItem>? items = null)
    {
        _items = (items ?? Enumerable.Empty<MediaItem>())
                 .Distinct()
                 .OrderBy(i => i.SortOrder)
                 .ToList();
        Guard.Range(_items.Count <= 50, "Maximum 50 media items allowed.");
    }

    public int CountOf(MediaType type) => _items.Count(i => i.Type == type);

    public MediaCollection Replace(IEnumerable<MediaItem> items) => new(items);

    protected override IEnumerable<object?> GetEqualityComponents()
    { foreach (var i in _items) yield return i; }
}
