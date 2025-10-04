using FrontendMentor.InvoiceApp.Domain.Entities;

namespace FrontendMentor.InvoiceApp.Domain.Tests.Entities;

public class CountryTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string name)
    {
        // Arrange
        const string code = "ZA";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Country.Create(name, code));
        exception.ParamName.Should().Be("name");
        exception.Message.Should().Contain("Country name cannot be null or empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCode_ShouldThrowArgumentException(string code)
    {
        // Arrange
        var name = "South Africa";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Country.Create(name, code));
        exception.ParamName.Should().Be("code");
        exception.Message.Should().Contain("Country code cannot be null or empty");
    }

    [Theory]
    [InlineData("A")]
    [InlineData("ABC")]
    [InlineData("ABCD")]
    public void Create_WithInvalidCodeLength_ShouldThrowArgumentException(string code)
    {
        // Arrange
        var name = "South Africa";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Country.Create(name, code));
        exception.ParamName.Should().Be("code");
        exception.Message.Should().Contain("Country code must be exactly 2 characters long");
    }

    [Fact]
    public void Create_WithValidInput_ShouldCreateCountry()
    {
        // Arrange
        var name = "South Africa";
        var code = "ZA";

        // Act
        var country = Country.Create(name, code);

        // Assert
        country.Should().NotBeNull();
        country.Name.Should().Be(name);
        country.Code.Should().Be("ZA");
        country.Id.Should().Be(Guid.Empty); // EF Core will generate the actual ID
    }

    [Fact]
    public void Create_WithLowerCaseCode_ShouldConvertToUpperCase()
    {
        // Arrange
        var name = "South Africa";
        var code = "za";

        // Act
        var country = Country.Create(name, code);

        // Assert
        country.Code.Should().Be("ZA");
    }

    [Fact]
    public void Create_WithWhitespaceInName_ShouldTrimWhitespace()
    {
        // Arrange
        var name = "  South Africa  ";
        var code = "ZA";

        // Act
        var country = Country.Create(name, code);

        // Assert
        country.Name.Should().Be("South Africa");
    }

    [Fact]
    public void Create_WithNullName_ShouldThrowArgumentException()
    {
        // Arrange
        string name = null!;
        var code = "ZA";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Country.Create(name, code));
        exception.ParamName.Should().Be("name");
        exception.Message.Should().Contain("Country name cannot be null or empty");
    }

    [Fact]
    public void Create_WithNullCode_ShouldThrowArgumentException()
    {
        // Arrange
        var name = "South Africa";
        string code = null!;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Country.Create(name, code));
        exception.ParamName.Should().Be("code");
        exception.Message.Should().Contain("Country code cannot be null or empty");
    }

    [Theory]
    [InlineData("12")]
    [InlineData("A1")]
    [InlineData("1A")]
    [InlineData("A-")]
    [InlineData("@A")]
    public void Create_WithNonLetterCode_ShouldThrowArgumentException(string code)
    {
        // Arrange
        var name = "South Africa";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Country.Create(name, code));
        exception.ParamName.Should().Be("code");
        exception.Message.Should().Contain("Country code must contain only letters");
    }
}
