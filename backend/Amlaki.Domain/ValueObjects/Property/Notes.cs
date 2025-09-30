namespace Domain.ValueObjects.Property;

using Domain.Primitives;

public sealed class Notes : ValueObject
{
    public string PublicText { get; }
    public string? InternalNotes { get; }

    public Notes(string? publicText, string? internalNotes)
    {
        PublicText = (publicText ?? string.Empty).Trim();
        InternalNotes = string.IsNullOrWhiteSpace(internalNotes) ? null : internalNotes.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return PublicText; yield return InternalNotes; }
}
