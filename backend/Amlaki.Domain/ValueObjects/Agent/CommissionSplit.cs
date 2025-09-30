namespace Domain.ValueObjects.Agent;

using Domain.Primitives;

public sealed class CommissionSplit : ValueObject
{
    public decimal AgentPercent { get; }
    public decimal BrokeragePercent { get; }
    public decimal PlatformPercent { get; }

    public CommissionSplit(decimal agentPercent, decimal brokeragePercent, decimal platformPercent)
    {
        Guard.Range(agentPercent >= 0 && brokeragePercent >= 0 && platformPercent >= 0, "Commission percents cannot be negative.");
        var sum = agentPercent + brokeragePercent + platformPercent;
        Guard.Range(sum == 100m, "Commission split must sum to 100%.");
        Guard.Range(agentPercent <= 95m, "Agent share too high per policy.");
        AgentPercent = agentPercent;
        BrokeragePercent = brokeragePercent;
        PlatformPercent = platformPercent;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return AgentPercent; yield return BrokeragePercent; yield return PlatformPercent; }
}
