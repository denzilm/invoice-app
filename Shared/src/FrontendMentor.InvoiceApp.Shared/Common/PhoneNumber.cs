using PhoneNumbers;

namespace FrontendMentor.InvoiceApp.Shared.Common;

public sealed record PhoneNumber
{
    private static readonly IPhoneNumberUtil DefaultUtil = new LibPhoneNumberUtil();

    private PhoneNumber(string number, string? countryCode)
    {
        Number = number;
        CountryCode = countryCode;
    }

    /// <summary>
    /// E.164-formatted phone number, e.g. "+12025551234"
    /// </summary>
    public string Number { get; set; }

    /// <summary>
    /// ISO 3166-1 alpha-2 region code, e.g. "US". May be null for certain
    /// valid number types (shared-cost, satellite) that have no regional assignment
    /// </summary>
    public string? CountryCode { get; set; }

    public static PhoneNumber Create(string phoneNumber, string? defaultRegion = null) =>
        Create(phoneNumber, defaultRegion, DefaultUtil);

    public static bool TryCreate(string phoneNumber, string? defaultRegion, out PhoneNumber? number) =>
        TryCreate(phoneNumber, defaultRegion, DefaultUtil, out number, out _);

    internal static PhoneNumber Create(string phoneNumber, string? defaultRegion, IPhoneNumberUtil util)
    {
        if (!TryCreate(phoneNumber, defaultRegion, util, out var number, out var error))
            throw new ArgumentException(error, nameof(phoneNumber));

        return number!;
    }

    internal static bool TryCreate(
        string phoneNumber, string? defaultRegion, IPhoneNumberUtil phoneNumberUtil, out PhoneNumber? number) =>
        TryCreate(phoneNumber, defaultRegion, phoneNumberUtil, out number, out _);

    private static bool TryCreate(
        string phoneNumber, string? defaultRegion, IPhoneNumberUtil phoneNumberUtil, out PhoneNumber? number, out string? error)
    {
        number = null;

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            error = "Phone number cannot be null or empty";
            return false;
        }

        var trimmedPhoneNumber = phoneNumber.Trim();

        PhoneNumbers.PhoneNumber parsedNumber;
        try
        {
            parsedNumber = phoneNumberUtil.Parse(trimmedPhoneNumber, defaultRegion ?? "US");
        }
        catch (NumberParseException ex)
        {
            error = ex.ErrorType switch
            {
                ErrorType.TOO_SHORT_NSN => "Phone number is too short",
                ErrorType.TOO_LONG => "Phone number is too long",
                _ => "Invalid phone number format"
            };

            return false;
        }
        catch
        {
            error = "Invalid phone number format";
            return false;
        }

        if (!phoneNumberUtil.IsValidNumber(parsedNumber))
        {
            error = "Phone number is not valid";
            return false;
        }

        var formattedNumber = phoneNumberUtil.Format(parsedNumber, PhoneNumberFormat.E164);
        var countryCode = phoneNumberUtil.GetRegionCodeForPhoneNumber(parsedNumber);

        number = new PhoneNumber(formattedNumber, countryCode);
        error = null;
        return true;
    }

    /// <summary>
    /// Returns the E.164 number string directly
    /// </summary>
    public override string ToString() => Number;

    /// <summary>
    /// Allows passing a <see cref="PhoneNumber"/> wherever a string is expected
    /// </summary>
    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Number;

    /// <summary>
    /// Explicit cast from string - throws <see cref="ArgumentException"/> on invalid input
    /// </summary>
    public static explicit operator PhoneNumber(string number) => Create(number);
}
