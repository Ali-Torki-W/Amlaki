namespace Domain.Entities.Transaction;

using Domain.Primitives;
using Domain.Exceptions;
using Domain.Enums.Transaction;
using Domain.Enums.Property; // reuse TransactionType from Property domain
using Domain.ValueObjects.Property; // reuse Price
using Domain.ValueObjects.Shared;   // Money
using Domain.ValueObjects.Transaction;
using System;

public sealed class Transaction : AggregateRoot<Guid>
{
    // Foreign keys (GUIDs kept infra-agnostic)
    public Guid PropertyId { get; private set; }
    public Guid SellerId { get; private set; }
    public Guid? BuyerId { get; private set; }

    // Core descriptors
    public TransactionType Type { get; private set; }     // Sale/Rent/Lease/Auction (reused)
    public DealStatus Status { get; private set; }        // Transaction lifecycle, distinct from listing lifecycle
    public Price AgreedPrice { get; private set; }        // Final price agreed for the deal

    // Commissions (deal-time)
    public Money AgentCommission { get; private set; }    // agent/broker
    public Money AmlakiCommission { get; private set; }   // platform/union/etc.

    // Artifacts & ledger
    public ContractInfo Contract { get; private set; }
    public PaymentLedger Payments { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Transaction() : base(Guid.Empty) { } // ORM

    private Transaction(
        Guid propertyId,
        Guid sellerId,
        TransactionType type,
        Price tentativePrice,
        Money agentCommission,
        Money amlakiCommission) : base(Guid.NewGuid())
    {
        if (propertyId == Guid.Empty) throw new DomainException("PropertyId is required.");
        if (sellerId == Guid.Empty) throw new DomainException("SellerId is required.");
        if (tentativePrice.Value.IsNegative) throw new DomainException("Price cannot be negative.");
        if (agentCommission.IsNegative) throw new DomainException("Agent commission cannot be negative.");
        if (amlakiCommission.IsNegative) throw new DomainException("Amlaki commission cannot be negative.");

        PropertyId = propertyId;
        SellerId = sellerId;
        Type = type;

        // At creation we only have a tentative/asking price; transaction isn’t bound yet
        AgreedPrice = tentativePrice;
        AgentCommission = agentCommission;
        AmlakiCommission = amlakiCommission;

        Status = DealStatus.Initiated;                    // draft-like state for a deal
        Contract = ContractInfo.Empty();
        Payments = PaymentLedger.Empty();

        CreatedAtUtc = UpdatedAtUtc = DateTime.UtcNow;
    }

    public static Transaction Start(
        Guid propertyId,
        Guid sellerId,
        TransactionType type,
        Price tentativePrice,
        Money agentCommission,
        Money amlakiCommission)
        => new(propertyId, sellerId, type, tentativePrice, agentCommission, amlakiCommission);

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;

    // ===== Lifecycle =====

    public void ProposeOffer(Guid buyerId, Price offerPrice)
    {
        if (Status != DealStatus.Initiated && Status != DealStatus.OfferRejected)
            throw new DomainException("Offer can only be proposed from Initiated or after rejection.");
        if (buyerId == Guid.Empty) throw new DomainException("BuyerId is required.");
        if (offerPrice.Value.IsNegative) throw new DomainException("Offer price cannot be negative.");

        BuyerId = buyerId;
        AgreedPrice = offerPrice; // current best proposal (not final)
        Status = DealStatus.OfferProposed;
        Touch();
    }

    public void AcceptOffer()
    {
        if (Status != DealStatus.OfferProposed)
            throw new DomainException("Only a proposed offer can be accepted.");
        Status = DealStatus.OfferAccepted;
        Touch();
    }

    public void RejectOffer()
    {
        if (Status != DealStatus.OfferProposed)
            throw new DomainException("Only a proposed offer can be rejected.");
        // keep BuyerId for audit, but you may null it if you prefer
        Status = DealStatus.OfferRejected;
        Touch();
    }

    public void SignContract(string contractNumber, Uri? documentUrl)
    {
        if (Status != DealStatus.OfferAccepted)
            throw new DomainException("Contract can be signed only after the offer is accepted.");

        Contract = ContractInfo.Signed(contractNumber, documentUrl, DateTime.UtcNow);
        Status = DealStatus.ContractSigned;
        Touch();
    }

    public void RecordPayment(Money amount, PaymentMethod method, string? reference = null)
    {
        if (Status is DealStatus.Canceled or DealStatus.Completed)
            throw new DomainException("Cannot record payments on canceled or completed transactions.");
        if (amount.IsNegative) throw new DomainException("Payment amount cannot be negative.");
        if (BuyerId is null) throw new DomainException("BuyerId must be set before payments.");

        Payments = Payments.Add(new PaymentEntry(amount, method, DateTime.UtcNow, reference));
        Touch();
    }

    public void MarkCompleted()
    {
        if (Status != DealStatus.ContractSigned && Status != DealStatus.PaymentInProgress)
            throw new DomainException("Deal must have a signed contract or ongoing payments to complete.");

        var totalDue = AgreedPrice.Value.Amount + AgentCommission.Amount + AmlakiCommission.Amount;
        if (Payments.TotalPaid.Amount < totalDue)
        {
            Status = DealStatus.PaymentInProgress; // partial payments recorded
            Touch();
            throw new DomainException("Cannot complete: outstanding balance remains.");
        }

        Status = DealStatus.Completed;
        Touch();
    }

    public void Cancel(CancellationReason reason, string? note = null)
    {
        if (Status is DealStatus.Completed or DealStatus.Canceled)
            throw new DomainException("Deal is already completed or canceled.");

        Status = DealStatus.Canceled;
        Touch();
        // you could persist reason/note in a separate VO if needed
    }
}
