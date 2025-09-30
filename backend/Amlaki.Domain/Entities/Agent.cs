namespace Domain.Entities.Agent;

using Domain.Primitives;
using Domain.Exceptions;
using Domain.Enums.Agent;
using Domain.ValueObjects.Agent;

public sealed class Agent : AggregateRoot<Guid>
{
    // Identity links
    public Guid UserId { get; private set; }  // cross-aggregate reference (User aggregate)

    // Compliance & readiness
    public VerificationSnapshot Verification { get; private set; } // snapshot at enrollment time, can be refreshed
    public AgentLicense License { get; private set; }
    public CommissionSplit Commission { get; private set; }
    public BrokerageAffiliation? Affiliation { get; private set; } // optional, but required to Activate in many markets
    public ServiceAreas ServiceAreas { get; private set; }
    public AgentDocuments Documents { get; private set; }

    // Lifecycle
    public AgentStatus Status { get; private set; }
    public string? SuspensionReason { get; private set; }

    // Audit
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Agent() : base(Guid.Empty) { }

    private Agent(
        Guid userId,
        VerificationSnapshot verification,
        AgentLicense license,
        CommissionSplit commission,
        ServiceAreas serviceAreas,
        AgentDocuments documents) : base(Guid.NewGuid())
    {
        if (userId == Guid.Empty) throw new DomainException("UserId is required.");
        if (!verification.MeetsAgentPrerequisites())
            throw new DomainException("User must have verified email/phone and approved KYC to enroll as Agent.");

        UserId = userId;
        Verification = verification;
        License = license;
        Commission = commission;
        ServiceAreas = serviceAreas;
        Documents = documents;

        Status = AgentStatus.Onboarding;
        SuspensionReason = null;
        CreatedAtUtc = UpdatedAtUtc = DateTime.UtcNow;
    }

    public static Agent Enroll(
        Guid userId,
        VerificationSnapshot verification,
        AgentLicense license,
        CommissionSplit commission,
        ServiceAreas serviceAreas,
        AgentDocuments documents)
        => new(userId, verification, license, commission, serviceAreas, documents);

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;

    // ===== Rules & Transitions =====

    public void AttachAffiliation(BrokerageAffiliation affiliation)
    {
        if (Status == AgentStatus.Deactivated) throw new DomainException("Deactivated agent cannot change affiliation.");
        if (Affiliation is { IsActive: true })
            throw new DomainException("An active affiliation already exists. End it before attaching a new one.");

        Affiliation = affiliation;
        Touch();
    }

    public void EndAffiliation(DateTime endUtc)
    {
        if (Affiliation is null || !Affiliation.IsActive) return;
        Affiliation = Affiliation.End(endUtc);
        Touch();
    }

    public void RefreshVerification(VerificationSnapshot snapshot)
    {
        if (Status == AgentStatus.Deactivated) throw new DomainException("Deactivated agent cannot update verification.");
        Verification = snapshot;
        Touch();
    }

    public void UpdateCommission(CommissionSplit split)
    {
        if (Status == AgentStatus.Deactivated) throw new DomainException("Deactivated agent cannot update commission.");
        Commission = split;
        Touch();
    }

    public void ReplaceServiceAreas(ServiceAreas areas)
    {
        if (Status == AgentStatus.Deactivated) throw new DomainException("Deactivated agent cannot update service areas.");
        ServiceAreas = areas;
        Touch();
    }

    public void AddDocument(AgentDocument doc)
    {
        if (Status == AgentStatus.Deactivated) throw new DomainException("Deactivated agent cannot add documents.");
        Documents = Documents.Add(doc);
        Touch();
    }

    public void RenewLicense(DateTime newExpiryUtc)
    {
        License = License.Renew(newExpiryUtc);
        Touch();
    }

    public void RevokeLicense()
    {
        License = License.Revoke();
        if (Status == AgentStatus.Active) Suspend("License revoked.");
        Touch();
    }

    public void Activate()
    {
        if (Status == AgentStatus.Active) return;

        // Hard business gates to go live
        if (!Verification.MeetsAgentPrerequisites())
            throw new DomainException("Cannot activate: user verification/KYC not satisfied.");
        if (!License.IsCurrentlyValid(DateTime.UtcNow))
            throw new DomainException("Cannot activate: license invalid or expired.");
        if (Affiliation is not null && !Affiliation.IsActive)
            throw new DomainException("Cannot activate: affiliation ended.");
        if (ServiceAreas.Areas.Count == 0)
            throw new DomainException("Cannot activate: at least one service area is required.");

        Documents.EnsureRequiredForActivation();

        Status = AgentStatus.Active;
        SuspensionReason = null;
        Touch();
    }

    public void Suspend(string reason)
    {
        if (Status == AgentStatus.Deactivated) throw new DomainException("Agent already deactivated.");
        Status = AgentStatus.Suspended;
        SuspensionReason = Guard.NotEmpty(reason, nameof(reason));
        Touch();
    }

    public void Reactivate()
    {
        if (Status == AgentStatus.Deactivated) throw new DomainException("Cannot reactivate a deactivated agent.");
        if (!License.IsCurrentlyValid(DateTime.UtcNow))
            throw new DomainException("Cannot reactivate: license invalid or expired.");
        if (!Verification.MeetsAgentPrerequisites())
            throw new DomainException("Cannot reactivate: user verification/KYC not satisfied.");

        Status = AgentStatus.Active;
        SuspensionReason = null;
        Touch();
    }

    public void Deactivate(string? reason = null)
    {
        Status = AgentStatus.Deactivated;
        SuspensionReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        Touch();
    }

    // Convenience guard for listing creation elsewhere
    public bool CanListPropertyNow() =>
        Status == AgentStatus.Active && License.IsCurrentlyValid(DateTime.UtcNow) && Verification.MeetsAgentPrerequisites();
}
