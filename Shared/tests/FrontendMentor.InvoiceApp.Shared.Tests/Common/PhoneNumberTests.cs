using FrontendMentor.InvoiceApp.Shared.Common;

namespace FrontendMentor.InvoiceApp.Shared.Tests.Common;

public sealed class PhoneNumberTests
{
    [Theory]
    [InlineData("+12025551234", null, "+12025551234", "US")]
    [InlineData("020 7946 0958", "GB", "+442079460958", "GB")]
    [InlineData("+61 2 9374 4000", null, "+61293744000", "AU")]
    [InlineData("+49 30 12345678", null, "+493012345678", "DE")]
    public void Create_ValidNumber_ReturnsNormalizedE164FormattedNumber(
        string input, string? region, string expectedNumber, string expectedCountry)
    {
        var result = PhoneNumber.Create(input, region);

        Assert.Equal(expectedNumber, result.Number);
        Assert.Equal(expectedCountry, result.CountryCode);
    }

    [Fact]
    public void Create_UsNUmberWithoutRegion_DefaultsToUS()
    {
        var result = PhoneNumber.Create("(202) 555-1234");

        Assert.Equal("US", result.CountryCode);
        Assert.StartsWith("+1", result.Number);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Empty_ThrowsWithCorrectParamName(string? input)
    {
        var ex =  Assert.Throws<ArgumentException>(() => PhoneNumber.Create(input!));

        Assert.Equal("phoneNumber", ex.ParamName);
        Assert.Contains("null or empty", ex.Message);
    }

    [Theory]
    [InlineData("notaphonenumber")]
    [InlineData("abc-def-ghij")]
    public void Create_UnparsableInput_ThrowsArgumentException(string input)
    {
        var ex = Assert.Throws<ArgumentException>(() => PhoneNumber.Create(input));

        Assert.Equal("phoneNumber", ex.ParamName);
    }

    [Fact]
    public void Create_StructurallyPlausibleButInvalidNumber_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => PhoneNumber.Create("12345678901234"));

        Assert.Equal("phoneNumber", ex.ParamName);
        Assert.Contains("not valid", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_AlwaysStoresE164Format()
    {
        var result = PhoneNumber.Create("(202) 555-1234", "US");

        Assert.Matches(@"^\+\d+$", result.Number);
    }

    [Fact]
    public void Create_ExplicitRegionOverridesAmbiguousLocalNumber()
    {
        var result = PhoneNumber.Create("03 1234 5678", "AU");

        Assert.Equal("AU", result.CountryCode);
    }

    [Fact]
    public void TryCreate_ValidNumber_ReturnsTrueAndOutputsPhoneNumber()
    {
        var success = PhoneNumber.TryCreate("+12025551234", null, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal("+12025551234", result.Number);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-number")]
    public void TryCreate_InvalidInput_ReturnsFalseWithNullOutput(string? input)
    {
        var success = PhoneNumber.TryCreate(input!, null, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TwoPhoneNumbers_SameE164Value_AreEqual()
    {
        var a = PhoneNumber.Create("+1 202 555 1234");
        var b = PhoneNumber.Create("(202) 555-1234", "US");

        Assert.Equal(a, b);
    }

    [Fact]
    public void TwoPhoneNumbers_DifferentValues_AreNotEqual()
    {
        var a = PhoneNumber.Create("+12025551234");
        var b = PhoneNumber.Create("+12025559999");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void WithExpression_ProducesNewInstanceWithUpdatedValue()
    {
        var original = PhoneNumber.Create("+12025551234");
        var modified  = original with { CountryCode = "CA" };

        Assert.Equal("+12025551234", modified.Number);
        Assert.Equal("CA", modified.CountryCode);
        // original is untouched
        Assert.Equal("US", original.CountryCode);
    }

    [Fact]
    public void ToString_ReturnsE164Number()
    {
        var phone = PhoneNumber.Create("+12025551234");

        Assert.Equal("+12025551234", phone.ToString());
    }

    [Fact]
    public void ImplicitStringConversion_ReturnsE164Number()
    {
        PhoneNumber phone = PhoneNumber.Create("+12025551234");
        string number = phone; // implicit

        Assert.Equal("+12025551234", number);
    }

    [Fact]
    public void ExplicitCastFromString_ValidNumber_Succeeds()
    {
        var phone = (PhoneNumber)"+12025551234";

        Assert.Equal("+12025551234", phone.Number);
    }

    [Fact]
    public void ExplicitCastFromString_InvalidNumber_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => (PhoneNumber)"not-a-number");
    }
}
