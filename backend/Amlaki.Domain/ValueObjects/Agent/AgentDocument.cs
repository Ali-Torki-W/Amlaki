namespace Domain.ValueObjects.Agent;

using Domain.Primitives;
using Domain.Enums.Agent;

public sealed class AgentDocument : ValueObject
{
    public DocumentType Type { get; }
    public string Url { get; }
    public DateTime UploadedAtUtc { get; }

    public AgentDocument(DocumentType type, string url, DateTime uploadedAtUtc)
    {
        Type = type;
        Url = Guard.NotEmpty(url, nameof(url));
        UploadedAtUtc = uploadedAtUtc;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return Type; yield return Url; yield return UploadedAtUtc; }
}

public sealed class AgentDocuments : ValueObject
{
    private readonly List<AgentDocument> _docs;
    public IReadOnlyCollection<AgentDocument> Docs => _docs.AsReadOnly();

    public AgentDocuments(IEnumerable<AgentDocument>? docs = null)
        => _docs = (docs ?? Array.Empty<AgentDocument>()).Distinct().ToList();

    public AgentDocuments Add(AgentDocument doc) => new(_docs.Append(doc));

    public bool Has(DocumentType type) => _docs.Any(d => d.Type == type);

    public void EnsureRequiredForActivation()
    {
        if (!Has(DocumentType.License)) throw new Domain.Exceptions.DomainException("License document is required.");
        if (!Has(DocumentType.IDProof)) throw new Domain.Exceptions.DomainException("ID proof is required.");
        if (!Has(DocumentType.ProfilePhoto)) throw new Domain.Exceptions.DomainException("Profile photo is required.");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    { foreach (var d in _docs) yield return d; }
}

