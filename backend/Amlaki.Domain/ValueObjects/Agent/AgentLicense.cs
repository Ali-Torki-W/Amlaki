namespace Domain.ValueObjects.Agent;

using Domain.Primitives;
using Domain.Enums.Agent;

public sealed class AgentLicense : ValueObject
{
    public string Number { get; }
    public string IssuingAuthority { get; }
    public DateTime IssuedAtUtc { get; }
    public DateTime ExpiresAtUtc { get; }
    public LicenseStatus Status { get; }

    public AgentLicense(string number, string issuingAuthority, DateTime issuedAtUtc, DateTime expiresAtUtc, LicenseStatus status)
    {
        Number = Guard.NotEmpty(number, nameof(number));
        IssuingAuthority = Guard.NotEmpty(issuingAuthority, nameof(issuingAuthority));
        Guard.Range(expiresAtUtc > issuedAtUtc, "License expiry must be after issue date.");
        IssuedAtUtc = issuedAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        Status = status;
    }

    public bool IsCurrentlyValid(DateTime nowUtc)
        => Status == LicenseStatus.Active && ExpiresAtUtc > nowUtc;

    public AgentLicense Renew(DateTime newExpiryUtc)
    {
        Guard.Range(newExpiryUtc > ExpiresAtUtc, "New expiry must be later than current expiry.");
        return new AgentLicense(Number, IssuingAuthority, IssuedAtUtc, newExpiryUtc, LicenseStatus.Active);
    }

    public AgentLicense Revoke() => new(Number, IssuingAuthority, IssuedAtUtc, ExpiresAtUtc, LicenseStatus.Revoked);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Number; yield return IssuingAuthority; yield return IssuedAtUtc; yield return ExpiresAtUtc; yield return Status;
    }
}
