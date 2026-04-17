using PhoneNumbers;

namespace FrontendMentor.InvoiceApp.Shared.Common;

internal sealed class LibPhoneNumberUtil : IPhoneNumberUtil
{
    private readonly PhoneNumberUtil _phoneNumberUtil = PhoneNumberUtil.GetInstance();

    public PhoneNumbers.PhoneNumber Parse(string phoneNumber, string defaultRegion) =>
        _phoneNumberUtil.Parse(phoneNumber, defaultRegion);

    public bool IsValidNumber(PhoneNumbers.PhoneNumber phoneNumber) =>
        _phoneNumberUtil.IsValidNumber(phoneNumber);

    public string Format(PhoneNumbers.PhoneNumber phoneNumber, PhoneNumberFormat format) =>
        _phoneNumberUtil.Format(phoneNumber, format);

    public string? GetRegionCodeForPhoneNumber(PhoneNumbers.PhoneNumber phoneNumber) =>
        _phoneNumberUtil.GetRegionCodeForNumber(phoneNumber);
}
