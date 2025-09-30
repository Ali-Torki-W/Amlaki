namespace Domain.ValueObjects.User;

using Domain.Primitives;

public sealed class PasswordHash : ValueObject
{
    // Domain never stores raw passwords; only the computed hash + metadata.
    public string Hash { get; }
    public string Algorithm { get; } // e.g., "PBKDF2-SHA256", "Argon2id"
    public DateTime CreatedAtUtc { get; }

    public PasswordHash(string hash, string algorithm, DateTime createdAtUtc)
    {
        Hash = Guard.NotEmpty(hash, nameof(hash));
        Algorithm = Guard.NotEmpty(algorithm, nameof(algorithm));
        CreatedAtUtc = createdAtUtc;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    { yield return Hash; yield return Algorithm; }
}
