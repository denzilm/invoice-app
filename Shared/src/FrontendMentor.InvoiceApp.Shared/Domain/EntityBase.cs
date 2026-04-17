namespace FrontendMentor.InvoiceApp.Shared.Domain;

public abstract class EntityBase<TId> : IEquatable<EntityBase<TId>>
{
    private readonly IList<IDomainEvent> _events = new List<IDomainEvent>();

    protected EntityBase(TId id) => Id = id;

    public TId Id { get; }
    public IReadOnlyList<IDomainEvent> Events => _events.AsReadOnly();

    protected void RaiseEvent(IDomainEvent @event) => _events.Add(@event);

    public void ClearEvents() => _events.Clear();

    public bool Equals(EntityBase<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        if (Id is null || other.Id is null) return false;

        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj) => Equals(obj as EntityBase<TId>);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(EntityBase<TId> left, EntityBase<TId> right) => Equals(left, right);
    public static bool operator !=(EntityBase<TId> left, EntityBase<TId> right) => !Equals(left, right);
}
