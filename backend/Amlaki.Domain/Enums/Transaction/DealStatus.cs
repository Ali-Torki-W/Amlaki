namespace Domain.Enums.Transaction;

public enum DealStatus
{
    Initiated,        // draft deal object
    OfferProposed,    // buyer proposed an offer
    OfferAccepted,    // seller accepted buyer's offer
    OfferRejected,    // seller rejected
    ContractSigned,   // both parties signed
    PaymentInProgress,// partial payments collected
    Completed,        // funds settled + transfer confirmed
    Canceled          // canceled for any reason
}
