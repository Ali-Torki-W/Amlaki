namespace Domain.ValueObjects.User;

using Domain.Primitives;

public sealed class KycProfile : ValueObject
{
    public PersonName Name { get; }
    public string NationalId { get; }     // country-specific validation can be added later
    public DateTime? BirthDate { get; }
    public string? AddressLine { get; }

    public KycProfile(PersonName name, string nationalId, DateTime? birthDate = null, string? addressLine = null)
    {
        Name = name;
        NationalId = Guard.NotEmpty(nationalId, nameof(nationalId));
        BirthDate = birthDate;
        AddressLine = string.IsNullOrWhiteSpace(addressLine) ? null : addressLine.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return Name; yield return NationalId; yield return BirthDate; yield return AddressLine; }
}
