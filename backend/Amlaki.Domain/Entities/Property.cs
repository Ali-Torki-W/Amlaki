namespace Domain.Entities.Property;

using Domain.Enums.Property;
using Domain.Exceptions;
using Domain.Primitives;
using Domain.ValueObjects.Property;
using Domain.ValueObjects.Shared;

public sealed class Property : AggregateRoot<Guid>
{
    public ListingCode Code { get; private set; }
    public TransactionType TransactionType { get; private set; }

    public PropertyStatus Status { get; private set; }               // availability
    public TransactionStatus TransactionStatus { get; private set; } // lifecycle
    public ModerationStatus Moderation { get; private set; }

    public Guid SellerId { get; private set; }
    public Guid? BuyerId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    // Details
    public PropertyType PropertyType { get; private set; }
    public Price Price { get; private set; }
    public AreaInfo Area { get; private set; }
    public Address Address { get; private set; }
    public Interior Interior { get; private set; }
    public Amenities Amenities { get; private set; }
    public MediaCollection Media { get; private set; }
    public Notes Notes { get; private set; }
    public Commission Commission { get; private set; }

    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();
    private readonly List<Tag> _tags = new();

    private Property() : base(Guid.Empty) { } // ORM only

    private Property(
        ListingCode code,
        TransactionType transactionType,
        Guid sellerId,
        PropertyType propertyType,
        Price price,
        AreaInfo area,
        Address address,
        Interior interior,
        Amenities amenities,
        MediaCollection media,
        Notes notes,
        Commission commission) : base(Guid.NewGuid())
    {
        Code = code;
        TransactionType = transactionType;
        SellerId = sellerId != Guid.Empty ? sellerId : throw new DomainException("SellerId is required.");

        PropertyType = propertyType;
        Price = price;
        Area = area;
        Address = address;
        Interior = interior;
        Amenities = amenities;
        Media = media;
        Notes = notes;
        Commission = commission;

        Status = PropertyStatus.OffMarket;
        TransactionStatus = TransactionStatus.Draft;
        Moderation = ModerationStatus.PendingReview;

        CreatedAtUtc = UpdatedAtUtc = DateTime.UtcNow;
    }

    public static Property Create(
        ListingCode code,
        TransactionType transactionType,
        Guid sellerId,
        PropertyType propertyType,
        Price price,
        AreaInfo area,
        Address address,
        Interior interior,
        Amenities amenities,
        MediaCollection media,
        Notes notes,
        Commission commission)
        => new(code, transactionType, sellerId, propertyType, price, area, address, interior, amenities, media, notes, commission);

    // ===== Behaviors with invariants =====

    public void ApproveModeration()
    {
        Moderation = ModerationStatus.Approved;
        Touch();
    }

    public void RejectModeration()
    {
        Moderation = ModerationStatus.Rejected;
        Unpublish(); // ensure not live
        Touch();
    }

    public void Publish()
    {
        if (TransactionStatus != TransactionStatus.Draft)
            throw new DomainException("Only draft properties can be published.");
        if (Price.Value.Amount <= 0)
            throw new DomainException("Price must be > 0 to publish.");
        if (Moderation != ModerationStatus.Approved)
            throw new DomainException("Listing must be approved before publishing.");
        if (Media.CountOf(MediaType.Photo) < 1)
            throw new DomainException("At least one photo is required to publish.");

        TransactionStatus = TransactionStatus.Published;
        Status = PropertyStatus.Available;
        Touch();
    }

    public void Unpublish()
    {
        if (TransactionStatus == TransactionStatus.Published)
        {
            TransactionStatus = TransactionStatus.Draft;
            Status = PropertyStatus.OffMarket;
            Touch();
        }
    }

    public void ChangePrice(Price newPrice)
    {
        if (TransactionStatus == TransactionStatus.Closed)
            throw new DomainException("Closed listings cannot change price.");
        if (newPrice.Value.IsNegative)
            throw new DomainException("Price cannot be negative.");

        if (!Equals(Price, newPrice))
        {
            Price = newPrice;
            Touch();
        }
    }

    public void UpdateAddress(Address newAddress)
    {
        if (TransactionStatus == TransactionStatus.Closed)
            throw new DomainException("Closed listings cannot change address.");
        Address = newAddress;
        Touch();
    }

    public void ReplaceMedia(MediaCollection media)
    {
        if (TransactionStatus == TransactionStatus.Closed)
            throw new DomainException("Closed listings cannot change media.");
        Media = media;
        Touch();
    }

    public void AddTag(Tag tag)
    {
        if (_tags.Contains(tag)) return;
        _tags.Add(tag);
    }

    public void RemoveTag(Tag tag) => _tags.Remove(tag);

    public void MarkAsSold(Guid buyerId)
    {
        if (TransactionStatus != TransactionStatus.Published)
            throw new DomainException("Only published listings can be sold.");
        if (Status == PropertyStatus.Sold)
            throw new DomainException("Property already sold.");
        if (buyerId == Guid.Empty)
            throw new DomainException("BuyerId is required.");

        BuyerId = buyerId;
        Status = PropertyStatus.Sold;
        TransactionStatus = TransactionStatus.Closed;
        Touch();
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
