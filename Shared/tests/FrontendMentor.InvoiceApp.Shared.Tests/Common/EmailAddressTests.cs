using FrontendMentor.InvoiceApp.Shared.Common;

namespace FrontendMentor.InvoiceApp.Shared.Tests.Common;

public sealed class EmailAddressTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]                        // case-insensitive
    [InlineData("User.Name+tag@sub.domain.org")]
    [InlineData("user123@domain.co.uk")]
    [InlineData("a@b.io")]                                  // minimal valid form
    [InlineData("user@xn--nxasmq6b.com")]                  // punycode domain
    [InlineData("\"quoted@name\"@example.com")]          // quoted local part
    [InlineData("\"user,name\"@example.com")]
    [InlineData("user!#$%&'*+/=?^_`{}|~@example.com")]    // special chars in local
    [InlineData("user@[192.168.1.1]")]
    [InlineData("user@[255.255.255.255]")]
    public void Create_WithValidEmail_ReturnsEmailAddress(string email)
    {
        var result = EmailAddress.Create(email);

        Assert.Equal(email.Trim(), result.Value);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]                        // case-insensitive
    [InlineData("User.Name+tag@sub.domain.org")]
    [InlineData("user123@domain.co.uk")]
    [InlineData("a@b.io")]                                  // minimal valid form
    [InlineData("user@xn--nxasmq6b.com")]                  // punycode domain
    [InlineData("\"quoted@name\"@example.com")]          // quoted local part
    [InlineData("\"user,name\"@example.com")]
    [InlineData("user!#$%&'*+/=?^_`{}|~@example.com")]    // special chars in local
    [InlineData("user@[192.168.1.1]")]
    [InlineData("user@[255.255.255.255]")]
    public void TryCreate_WithValidEmail_ReturnsEmailAddress(string email)
    {
        var result = EmailAddress.TryCreate(email, out var address);

        Assert.True(result);
        Assert.Equal(email.Trim(), address!.Value);
    }

    [Theory]
    [InlineData("  user@example.com  ")]
    [InlineData("\tuser@example.com\t")]
    [InlineData(" user@example.com")]
    public void Create_WithSurroundingWhitespace_TrimsAndSucceeds(string email)
    {
        var result = EmailAddress.Create(email);

        Assert.Equal(email.Trim(), result.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Create_WithNullOrWhitespace_ThrowsArgumentException(string? email)
    {
        var ex = Assert.Throws<ArgumentException>(() => EmailAddress.Create(email!));

        Assert.Equal("emailAddress", ex.ParamName);
        Assert.Contains("null or empty", ex.Message);
    }

    [Fact]
    public void Create_WithEmailExactlyAtMaxLength_Succeeds()
    {
        // 254 chars: local part padded so total == 254
        var email = $"{new string('a', 64)}@{new string('a', 178)}example.com"; // 64+1+178+11 = 254
        Assert.Equal(254, email.Length);

        var result = EmailAddress.Create(email);

        Assert.Equal(email, result.Value);
    }

    [Fact]
    public void Create_WithEmailOneCharOverMaxLength_ThrowsArgumentException()
    {
        var email = $"{new string('a', 244)}@example.com"; // 256 chars
        Assert.Equal(256, email.Length);

        var ex = Assert.Throws<ArgumentException>(() => EmailAddress.Create(email));

        Assert.Equal("emailAddress", ex.ParamName);
        Assert.Contains("254", ex.Message);
    }

    [Theory]
    [InlineData("plainaddress")]          // no @
    [InlineData("@missinglocalpart.com")] // empty local part
    [InlineData("user@")]                 // empty domain
    [InlineData("user@.com")]             // domain starts with dot
    [InlineData("user@com")]              // single-label domain (no dot)
    [InlineData("user @example.com")]     // space in local part
    [InlineData("user@@example.com")]     // double @
    [InlineData("user@exam ple.com")]     // space in domain
    [InlineData("user.@example.com")]     // trailing dot in local part
    [InlineData(".user@example.com")]     // leading dot in local part
    public void Create_WithInvalidFormat_ThrowsArgumentException(string email)
    {
        var ex = Assert.Throws<ArgumentException>(() => EmailAddress.Create(email));

        Assert.Equal("emailAddress", ex.ParamName);
        Assert.Contains("Invalid email address format", ex.Message);
    }

    [Fact]
    public void Value_ReturnsStoredEmail()
    {
        const string email = "value@example.com";

        var result = EmailAddress.Create(email);

        Assert.Equal(email, result.Value);
    }

    [Fact]
    public void ToString_ReturnsEmailString()
    {
        const string email = "tostring@example.com";

        var result = EmailAddress.Create(email).ToString();

        Assert.Equal(email, result);
    }

    [Fact]
    public void ImplicitStringConversion_ReturnsEmailString()
    {
        const string email = "implicit@example.com";
        var emailAddress = EmailAddress.Create(email);

        string result = emailAddress;

        Assert.Equal(email, result);
    }

    [Fact]
    public void TwoInstancesWithSameEmail_AreEqual()
    {
        var first = EmailAddress.Create("equal@example.com");
        var second = EmailAddress.Create("equal@example.com");

        Assert.Equal(first, second);
    }

    [Fact]
    public void TwoInstancesWithDifferentEmails_AreNotEqual()
    {
        var first = EmailAddress.Create("one@example.com");
        var second = EmailAddress.Create("two@example.com");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void TwoEqualInstances_HaveSameHashCode()
    {
        var first = EmailAddress.Create("hash@example.com");
        var second = EmailAddress.Create("hash@example.com");

        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }
}
