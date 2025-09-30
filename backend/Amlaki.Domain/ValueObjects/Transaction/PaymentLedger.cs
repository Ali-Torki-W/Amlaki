namespace Domain.ValueObjects.Transaction;

using Domain.Primitives;
using Domain.ValueObjects.Shared;

public sealed class PaymentLedger : ValueObject
{
    private readonly List<PaymentEntry> _entries;
    public IReadOnlyList<PaymentEntry> Entries => _entries.AsReadOnly();

    public Money TotalPaid { get; }

    private PaymentLedger(IEnumerable<PaymentEntry> entries)
    {
        _entries = entries.ToList();
        var total = _entries.Aggregate(0m, (acc, e) => acc + e.Amount.Amount);
        TotalPaid = new Money(total, _entries.FirstOrDefault()?.Amount.Currency ?? "IRR");
    }

    public static PaymentLedger Empty() => new(Array.Empty<PaymentEntry>());

    public PaymentLedger Add(PaymentEntry entry)
    {
        // ensure currency consistency
        if (Entries.Count > 0 && entry.Amount.Currency != TotalPaid.Currency)
            throw new Domain.Exceptions.DomainException("Payment currency mismatch.");
        return new PaymentLedger(_entries.Append(entry));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    { foreach (var e in _entries) yield return e; }
}
