using Microsoft.Extensions.Configuration;

namespace FrontendMentor.InvoiceApp.Shared.Hosting.Extensions;

public static class ConfigurationExtensions
{
    public static string GetRequiredValue(this IConfiguration configuration, string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));

        var section = configuration.GetSection(key);
        if (!section.Exists())
        {
            throw new InvalidOperationException($"Configuration key '{key}' is missing.");
        }

        var value = section.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Configuration key '{key}' has no value.");
        }

        return value;
    }
}
