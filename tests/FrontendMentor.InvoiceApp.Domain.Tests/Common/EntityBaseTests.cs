using FrontendMentor.InvoiceApp.Domain.Common;

namespace FrontendMentor.InvoiceApp.Domain.Tests.Common;

public sealed class EntityBaseTests
{
    [Fact]
    public void Equals_WithNullObject_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity(1);

        // Act
        var result = entity.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameReference_ShouldReturnTrue()
    {
        // Arrange
        var entity = new TestEntity(1);

        // Act
        var result = entity.Equals(entity);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new AnotherTestEntity(1);

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameTypeAndSameId_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(1);

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithSameTypeAndDifferentId_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(2);

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDefaultId_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity(0); // default int
        var entity2 = new TestEntity(0);

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithOneDefaultId_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(0); // default int

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithGuidIds_ShouldWorkCorrectly()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var entity1 = new GuidTestEntity(id1);
        var entity2 = new GuidTestEntity(id1);
        var entity3 = new GuidTestEntity(id2);

        // Act & Assert
        entity1.Equals(entity2).Should().BeTrue();
        entity1.Equals(entity3).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithStringIds_ShouldWorkCorrectly()
    {
        // Arrange
        var entity1 = new StringTestEntity("test-id");
        var entity2 = new StringTestEntity("test-id");
        var entity3 = new StringTestEntity("different-id");

        // Act & Assert
        entity1.Equals(entity2).Should().BeTrue();
        entity1.Equals(entity3).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithEmptyStringId_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = new StringTestEntity("");
        var entity2 = new StringTestEntity("");

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNullStringId_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new StringTestEntity(null!);
        var entity2 = new StringTestEntity(null!);

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameTypeAndId_ShouldReturnSameValue()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(1);

        // Act
        var hashCode1 = entity1.GetHashCode();
        var hashCode2 = entity2.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }

    [Fact]
    public void GetHashCode_WithDifferentIds_ShouldReturnDifferentValues()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(2);

        // Act
        var hashCode1 = entity1.GetHashCode();
        var hashCode2 = entity2.GetHashCode();

        // Assert
        hashCode1.Should().NotBe(hashCode2);
    }

    [Fact]
    public void GetHashCode_WithDifferentTypes_ShouldReturnDifferentValues()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new AnotherTestEntity(1);

        // Act
        var hashCode1 = entity1.GetHashCode();
        var hashCode2 = entity2.GetHashCode();

        // Assert
        hashCode1.Should().NotBe(hashCode2);
    }

    [Fact]
    public void EqualityOperator_WithSameEntities_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(1);

        // Act
        var result = entity1 == entity2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithDifferentEntities_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(2);

        // Act
        var result = entity1 == entity2;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_WithNullLeft_ShouldReturnFalse()
    {
        // Arrange
        TestEntity? entity1 = null;
        var entity2 = new TestEntity(1);

        // Act
        var result = entity1 == entity2;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_WithNullRight_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        TestEntity? entity2 = null;

        // Act
        var result = entity1 == entity2;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_WithBothNull_ShouldReturnTrue()
    {
        // Arrange
        TestEntity? entity1 = null;
        TestEntity? entity2 = null;

        // Act
        var result = entity1 == entity2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_WithSameEntities_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(1);

        // Act
        var result = entity1 != entity2;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithDifferentEntities_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(2);

        // Act
        var result = entity1 != entity2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_WithNullLeft_ShouldReturnTrue()
    {
        // Arrange
        TestEntity? entity1 = null;
        var entity2 = new TestEntity(1);

        // Act
        var result = entity1 != entity2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_WithNullRight_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        TestEntity? entity2 = null;

        // Act
        var result = entity1 != entity2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_WithBothNull_ShouldReturnFalse()
    {
        // Arrange
        TestEntity? entity1 = null;
        TestEntity? entity2 = null;

        // Act
        var result = entity1 != entity2;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Id_ShouldBeImmutable()
    {
        // Arrange
        var entity = new TestEntity(42);

        // Act & Assert
        entity.Id.Should().Be(42);
        
        // Verify that ID is read-only (this is a compile-time check, but we can verify the property exists)
        var idProperty = typeof(TestEntity).GetProperty("Id");
        idProperty.Should().NotBeNull();
        idProperty!.CanWrite.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithObjectParameter_ShouldWorkCorrectly()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(1);
        object objEntity2 = entity2;

        // Act
        var result = entity1.Equals(objEntity2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNonEntityObject_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity(1);
        var nonEntity = "not an entity";

        // Act
        var result = entity.Equals(nonEntity);

        // Assert
        result.Should().BeFalse();
    }
}

// Test entity classes for testing EntityBase
public sealed class TestEntity : EntityBase<int>
{
    public TestEntity(int id) : base(id) { }
}

public sealed class AnotherTestEntity : EntityBase<int>
{
    public AnotherTestEntity(int id) : base(id) { }
}

public sealed class GuidTestEntity : EntityBase<Guid>
{
    public GuidTestEntity(Guid id) : base(id) { }
}

public sealed class StringTestEntity : EntityBase<string>
{
    public StringTestEntity(string id) : base(id) { }
}
