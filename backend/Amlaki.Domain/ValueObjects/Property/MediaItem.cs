namespace Domain.ValueObjects.Property;

using Domain.Primitives;

public enum MediaType { Photo, Video, FloorPlan }

public sealed class MediaItem : ValueObject
{
    public string Url { get; }
    public MediaType Type { get; }
    public int SortOrder { get; }

    public MediaItem(string url, MediaType type, int sortOrder = 0)
    {
        Url = Guard.NotEmpty(url, nameof(url));
        Type = type;
        SortOrder = sortOrder;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return Url; yield return Type; }
}
