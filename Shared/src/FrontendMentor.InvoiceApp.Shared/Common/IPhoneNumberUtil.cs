using PhoneNumbers;

namespace FrontendMentor.InvoiceApp.Shared.Common;

internal interface IPhoneNumberUtil
{
    PhoneNumbers.PhoneNumber Parse(string phoneNumber, string defaultRegion);
    bool IsValidNumber(PhoneNumbers.PhoneNumber phoneNumber);
    string Format(PhoneNumbers.PhoneNumber phoneNumber, PhoneNumberFormat format);
    string? GetRegionCodeForPhoneNumber(PhoneNumbers.PhoneNumber phoneNumber);
}
