namespace Domain.Enums.Transaction;

public enum CancellationReason
{
    BuyerWithdrawn,
    SellerWithdrawn,
    FinancingFailed,
    ComplianceIssue,
    Other
}
