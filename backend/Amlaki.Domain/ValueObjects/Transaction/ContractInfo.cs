namespace Domain.ValueObjects.Transaction;

using Domain.Primitives;

public sealed class ContractInfo : ValueObject
{
    public string? ContractNumber { get; }
    public Uri? DocumentUrl { get; }
    public DateTime? SignedAtUtc { get; }

    private ContractInfo(string? number, Uri? url, DateTime? signedAtUtc)
    {
        ContractNumber = number;
        DocumentUrl = url;
        SignedAtUtc = signedAtUtc;
    }

    public static ContractInfo Empty() => new(null, null, null);

    public static ContractInfo Signed(string number, Uri? url, DateTime atUtc)
    {
        Guard.NotEmpty(number, nameof(number));
        return new ContractInfo(number.Trim(), url, atUtc);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return ContractNumber; yield return DocumentUrl?.ToString(); yield return SignedAtUtc; }
}
