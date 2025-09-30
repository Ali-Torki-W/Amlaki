namespace Domain.Primitives;

public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; }
    protected Entity(TId id) => Id = id;

    public override bool Equals(object? obj) => obj is Entity<TId> e && EqualityComparer<TId>.Default.Equals(Id, e.Id);
    public override int GetHashCode() => Id!.GetHashCode();
}
