namespace FrontendMentor.InvoiceApp.Domain.Common;

public abstract class EntityBase<TId> where TId : IComparable<TId>
{
    protected EntityBase(TId id) => Id = id;

    public TId Id { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not EntityBase<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != obj.GetType() || Equals(Id, default(TId)) || Equals(other.Id, default(TId))) return false;

        return Equals(Id, other.Id);
    }

    public override int GetHashCode() => (GetType().ToString() + Id).GetHashCode();

    public static bool operator ==(EntityBase<TId>? a, EntityBase<TId>? b) => Equals(a, b);
    public static bool operator !=(EntityBase<TId>? a, EntityBase<TId>? b) => !(a == b);
}
