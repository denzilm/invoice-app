using FrontendMentor.InvoiceApp.Domain.Common;

namespace FrontendMentor.InvoiceApp.Domain.Entities;

public sealed class Country : EntityBase<Guid>
{
    private Country(Guid id, string name, string code)
        : base(id)
    {
        Name = name;
        Code = code;
    }

    public string Name { get; private set; }
    public string Code { get; private set; }

    public static Country Create(string name, string code)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Country name cannot be null or empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Country code cannot be null or empty.", nameof(code));

        if (code.Length != 2)
            throw new ArgumentException("Country code must be exactly 2 characters long.", nameof(code));

        if (!code.All(char.IsLetter))
            throw new ArgumentException("Country code must contain only letters.", nameof(code));

        return new Country(Guid.Empty, name.Trim(), code.ToUpperInvariant());
    }
}
