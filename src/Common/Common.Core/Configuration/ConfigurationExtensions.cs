using Microsoft.Extensions.Configuration;

namespace Common.Core.Configuration;

public static class ConfigurationExtensions
{
    public static TOptions Create<TOptions>(this IConfigurationRoot configuration, string position)
        where TOptions : new()
    {
        var options = new TOptions();
        configuration.GetSection(position).Bind(options);

        return options;
    }
}
