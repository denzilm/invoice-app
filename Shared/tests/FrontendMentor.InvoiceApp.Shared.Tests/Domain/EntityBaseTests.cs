using FrontendMentor.InvoiceApp.Shared.Domain;

namespace FrontendMentor.InvoiceApp.Shared.Tests.Domain;

public sealed class EntityBaseTests
{
    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        var entity = new TestEntity("1");

        Assert.True(entity.Equals(entity));
    }

    [Fact]
    public void Equals_SameId_ReturnsTrue()
    {
        var a = new TestEntity("1");
        var b = new TestEntity("1");

        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_DifferentId_ReturnsFalse()
    {
        var a = new TestEntity("1");
        var b = new TestEntity("1");

        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_NullOther_ReturnsFalse()
    {
        var a = new TestEntity("1");

        Assert.False(a.Equals(null));
    }

    [Fact]
    public void Equals_NullId_ReturnsFalse()
    {
        var a = new TestEntity(null!);
        var b = new TestEntity("1");

        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equals_BothNullId_ReturnsFalse()
    {
        var a = new TestEntity(null!);
        var b = new TestEntity(null!);

        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equals_DifferentEntityType_SameId_ReturnsFalse()
    {
        var a = new TestEntity("1");
        var b = new OtherEntity("1");
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equals_ObjectOverload_SameId_ReturnsTrue()
    {
        var a = new TestEntity("1");
        object b = new TestEntity("1");
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_ObjectOverload_NonEntityType_ReturnsFalse()
    {
        var entity = new TestEntity("1");
        // ReSharper disable once SuspiciousTypeConversion.Global
        Assert.False(entity.Equals("1"));
    }

    [Fact]
    public void EqualityOperator_SameId_ReturnsTrue()
    {
        var a = new TestEntity("1");
        var b = new TestEntity("1");
        Assert.True(a == b);
    }

    [Fact]
    public void EqualityOperator_DifferentId_ReturnsFalse()
    {
        var a = new TestEntity("1");
        var b = new TestEntity("2");
        Assert.False(a == b);
    }

    [Fact]
    public void InequalityOperator_DifferentId_ReturnsTrue()
    {
        var a = new TestEntity("1");
        var b = new TestEntity("2");
        Assert.True(a != b);
    }

    [Fact]
    public void EqualityOperator_NullLeft_ReturnsFalse()
    {
        TestEntity? a = null;
        var b = new TestEntity("1");
        Assert.False(a! == b);
    }

    [Fact]
    public void EqualityOperator_NullRight_ReturnsFalse()
    {
        var a = new TestEntity("1");
        TestEntity? b = null;
        Assert.False(a == b!);
    }

    [Fact]
    public void EqualityOperator_BothNull_ReturnsTrue()
    {
        TestEntity? a = null;
        TestEntity? b = null;
        Assert.True(a! == b!);
    }

    [Fact]
    public void GetHashCode_SameId_ReturnsSameHash()
    {
        var a = new TestEntity("1");
        var b = new TestEntity("1");
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentId_ReturnsDifferentHash()
    {
        var a = new TestEntity("1");
        var b = new TestEntity("2");
        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentEntityType_SameId_ReturnsDifferentHash()
    {
        var a = new TestEntity("1");
        var b = new OtherEntity("1");
        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_IsConsistentAcrossCalls()
    {
        var entity = new TestEntity("1");
        Assert.Equal(entity.GetHashCode(), entity.GetHashCode());
    }

    [Fact]
    public void RaiseEvent_AddsEventToCollection()
    {
        var entity = new TestEntity("1");
        var @event = new TestDomainEvent(DateTimeOffset.Now);

        entity.Raise(@event);

        Assert.Single(entity.Events);
        Assert.Contains(@event, entity.Events);
    }

    [Fact]
    public void RaiseEvent_MultipleEvents_AllAppended()
    {
        var entity = new TestEntity("1");
        var first = new TestDomainEvent(DateTimeOffset.Now);
        var second = new TestDomainEvent(DateTimeOffset.Now);

        entity.Raise(first);
        entity.Raise(second);

        Assert.Equal(2, entity.Events.Count);
        Assert.Equal(first, entity.Events[0]);
        Assert.Equal(second, entity.Events[1]);
    }

    [Fact]
    public void ClearEvents_RemovesAllEvents()
    {
        var entity = new TestEntity("1");
        entity.Raise(new TestDomainEvent(DateTimeOffset.Now));
        entity.Raise(new TestDomainEvent(DateTimeOffset.Now));

        entity.ClearEvents();

        Assert.Empty(entity.Events);
    }

    [Fact]
    public void Events_IsReadOnly_CannotBeModifiedExternally()
    {
        var entity = new TestEntity("1");
        Assert.IsType<IReadOnlyList<IDomainEvent>>(entity.Events, exactMatch: false);
    }

    [Fact]
    public void Distinct_DeduplicatesEntitiesWithSameId()
    {
        var entities = new List<TestEntity>
        {
            new("1"),
            new("1"),
            new("2")
        };

        var distinct = entities.Distinct().ToList();

        Assert.Equal(2, distinct.Count);
    }

    [Fact]
    public void Contains_FindsEntityById()
    {
        var entities = new List<TestEntity> { new("1"), new("2") };
        var target = new TestEntity("1");

        Assert.Contains(target, entities);
    }

    private sealed class TestEntity(string id) : EntityBase<string>(id)
    {
        public void Raise(IDomainEvent @event) => RaiseEvent(@event);
    }

    private class OtherEntity(string id) : EntityBase<string>(id);

    private sealed record TestDomainEvent(DateTimeOffset OccurredOn) : IDomainEvent;
}
