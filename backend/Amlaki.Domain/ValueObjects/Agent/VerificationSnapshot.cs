namespace Domain.ValueObjects.Agent;

using Domain.Primitives;

public sealed class VerificationSnapshot : ValueObject
{
    public bool EmailVerified { get; }
    public bool PhoneVerified { get; }
    public bool KycApproved { get; }

    public VerificationSnapshot(bool emailVerified, bool phoneVerified, bool kycApproved)
    {
        EmailVerified = emailVerified;
        PhoneVerified = phoneVerified;
        KycApproved = kycApproved;
    }

    public bool MeetsAgentPrerequisites() => EmailVerified && PhoneVerified && KycApproved;

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return EmailVerified; yield return PhoneVerified; yield return KycApproved; }
}
