namespace Domain.ValueObjects.Transaction;

using Domain.Primitives;
using Domain.Enums.Transaction;
using Domain.ValueObjects.Shared;

public sealed class PaymentEntry : ValueObject
{
    public Money Amount { get; }
    public PaymentMethod Method { get; }
    public DateTime PaidAtUtc { get; }
    public string? Reference { get; }

    public PaymentEntry(Money amount, PaymentMethod method, DateTime paidAtUtc, string? reference = null)
    {
        Guard.Range(!amount.IsNegative, "Payment amount cannot be negative.");
        Amount = amount;
        Method = method;
        PaidAtUtc = paidAtUtc;
        Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return Amount; yield return Method; yield return PaidAtUtc; yield return Reference; }
}
