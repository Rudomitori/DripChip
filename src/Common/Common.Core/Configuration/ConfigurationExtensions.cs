using Microsoft.Extensions.Configuration;

namespace Common.Core.Configuration;

public static class ConfigurationExtensions
{
    public static TOptions Create<TOptions>(this IConfigurationRoot configuration)
        where TOptions : IPositionedOptions, new()
    {
        var options = new TOptions();
        configuration.GetSection(TOptions.Position).Bind(options);

        return options;
    }
}
