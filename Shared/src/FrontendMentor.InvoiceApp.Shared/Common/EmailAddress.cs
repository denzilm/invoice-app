using System.Text.RegularExpressions;

namespace FrontendMentor.InvoiceApp.Shared.Common;

public sealed partial record EmailAddress
{
    private const string EmailPattern =
        "(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])";
    private const int MaxEmailLength = 254; // as per RFC 5321

    [GeneratedRegex($"^{EmailPattern}$", RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 250)]
    private static partial Regex EmailRegex();

    private EmailAddress(string emailAddress)
    {
        Value = emailAddress;
    }

    public string Value { get; }

    public static EmailAddress Create(string emailAddress)
    {
        if (!TryCreate(emailAddress, out var result, out var error))
            throw new ArgumentException(error, nameof(emailAddress));
        return result!;
    }

    public static bool TryCreate(string emailAddress, out EmailAddress? result)
        => TryCreate(emailAddress, out result, out _);

    private static bool TryCreate(string emailAddress, out EmailAddress? result, out string? error)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            error = "Email address cannot be null or empty";
            return false;
        }

        var trimmed = emailAddress.Trim();
        if (trimmed.Length > MaxEmailLength)
        {
            error = $"Email address cannot exceed {MaxEmailLength} characters";
            return false;
        }

        var atIndex = trimmed.IndexOf('@');
        if (atIndex > 64)
        {
            error = "Email local part cannot exceed 64 characters";
            return false;
        }

        try
        {
            if (!EmailRegex().IsMatch(trimmed))
                throw new ArgumentException("Invalid email address format", nameof(emailAddress));
        }
        catch (RegexMatchTimeoutException)
        {
            error = "Email address validation timed out";
            return false;
        }

        result = new EmailAddress(trimmed);
        error = null;
        return true;
    }

    public override string ToString() => Value;

    public static implicit operator string(EmailAddress emailAddress) => emailAddress.Value;
}
