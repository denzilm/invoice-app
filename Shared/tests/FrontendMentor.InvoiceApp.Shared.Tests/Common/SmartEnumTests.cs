using FrontendMentor.InvoiceApp.Shared.Common;

namespace FrontendMentor.InvoiceApp.Shared.Tests.Common;

public sealed class SmartEnumTests
{
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_ShouldThrow_WhenNameIsNullOrEmpty(string? name)
    {
        Assert.Throws<ArgumentException>(() => new InvalidEnum(1, name!, "DisplayName"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_ShouldThrow_WhenDisplayNameIsNullOrEmpty(string? displayName)
    {
        Assert.Throws<ArgumentException>(() => new InvalidEnum(1, "Inactive", displayName!));
    }

    [Fact]
    public void Enumerate_ShouldReturnAllDefinedInstances()
    {
        var items = TestStatus.Enumerate();

        Assert.Contains(TestStatus.Active, items);
        Assert.Contains(TestStatus.Inactive, items);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public void Enumerate_ShouldReturnSameCachedInstance()
    {
        var first = TestStatus.Enumerate();
        var second = TestStatus.Enumerate();

        Assert.Same(first, second); // verifies caching
    }

    [Fact]
    public void FromValue_ShouldReturnCorrectItem()
    {
        var result = TestStatus.FromValue(1);

        Assert.Equal(TestStatus.Active, result);
    }

    [Fact]
    public void FromValue_ShouldThrow_WhenValueInvalid()
    {
        Assert.Throws<ArgumentException>(() => TestStatus.FromValue(999));
    }

    [Fact]
    public void TryGetFromValue_ShouldReturnTrue_WhenFound()
    {
        var success = TestStatus.TryGetFromValue(1, out var result);

        Assert.True(success);
        Assert.Equal(TestStatus.Active, result);
    }

    [Fact]
    public void TryGetFromValue_ShouldReturnFalse_WhenNotFound()
    {
        var success = TestStatus.TryGetFromValue(999, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void FromName_ShouldReturnCorrectItem()
    {
        var result = TestStatus.FromName("Active");

        Assert.Equal(TestStatus.Active, result);
    }

    [Fact]
    public void FromName_ShouldThrow_WhenNameInvalid()
    {
        Assert.Throws<ArgumentException>(() => TestStatus.FromName("Unknown"));
    }

    [Fact]
    public void FromName_ShouldThrow_WhenNameNullOrWhitespace()
    {
        Assert.Throws<ArgumentException>(() => TestStatus.FromName(null!));
        Assert.Throws<ArgumentException>(() => TestStatus.FromName(""));
    }

    [Fact]
    public void TryGetFromName_ShouldReturnTrue_WhenFound()
    {
        var success = TestStatus.TryGetFromName("Active", out var result);

        Assert.True(success);
        Assert.Equal(TestStatus.Active, result);
    }

    [Fact]
    public void TryGetFromName_ShouldReturnFalse_WhenInvalid()
    {
        var success = TestStatus.TryGetFromName("Unknown", out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryGetFromName_ShouldReturnFalse_WhenNullOrWhitespace()
    {
        Assert.False(TestStatus.TryGetFromName(null!, out var result1));
        Assert.Null(result1);

        Assert.False(TestStatus.TryGetFromName("", out var result2));
        Assert.Null(result2);
    }

    [Fact]
    public void Equals_ShouldReturnTrue_ForSameInstance()
    {
        Assert.True(TestStatus.Active.Equals(TestStatus.Active));
    }

    [Fact]
    public void Equals_ShouldReturnTrue_ForSameValue()
    {
        var anotherReference = TestStatus.FromValue(1);

        Assert.True(TestStatus.Active.Equals(anotherReference));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForDifferentValues()
    {
        Assert.False(TestStatus.Active.Equals(TestStatus.Inactive));
    }

    [Fact]
    public void EqualityOperator_ShouldWorkCorrectly()
    {
        var a = TestStatus.Active;
        var b = TestStatus.Active;
        Assert.True(a == b);
        Assert.True(TestStatus.Active != TestStatus.Inactive);
    }
}

internal sealed class TestStatus : SmartEnum<TestStatus>
{
    public static readonly TestStatus Active = new(1, "Active", "Active Display");
    public static readonly TestStatus Inactive = new(2, "Inactive", "Inactive Display");
    public TestStatus(int value, string name, string displayName)
        : base(value, name, displayName) { }

}

internal sealed class InvalidEnum : SmartEnum<InvalidEnum>
{
    public InvalidEnum(int value, string name, string displayName)
        : base(value, name, displayName)
    {
    }
}
