namespace Domain.ValueObjects.Agent;

using Domain.Primitives;

public sealed class BrokerageAffiliation : ValueObject
{
    public Guid BrokerageId { get; }
    public string BrokerageName { get; }
    public DateTime StartedAtUtc { get; }
    public DateTime? EndedAtUtc { get; }

    public bool IsActive => EndedAtUtc is null;

    public BrokerageAffiliation(Guid brokerageId, string brokerageName, DateTime startedAtUtc, DateTime? endedAtUtc = null)
    {
        Guard.Range(brokerageId != Guid.Empty, "BrokerageId is required.");
        BrokerageId = brokerageId;
        BrokerageName = Guard.NotEmpty(brokerageName, nameof(brokerageName));
        StartedAtUtc = startedAtUtc;
        if (endedAtUtc is not null) Guard.Range(endedAtUtc > startedAtUtc, "End date must be after start date.");
        EndedAtUtc = endedAtUtc;
    }

    public BrokerageAffiliation End(DateTime endUtc) => new(BrokerageId, BrokerageName, StartedAtUtc, endUtc);

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return BrokerageId; yield return BrokerageName; yield return StartedAtUtc; yield return EndedAtUtc; }
}
