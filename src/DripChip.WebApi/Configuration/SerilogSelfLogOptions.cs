using Common.Core.Configuration;

namespace DripChip.WebApi.Configuration;

public sealed class SerilogSelfLogOptions : IPositionedOptions
{
    public static string Position => "Serilog:SelfLog";
    public string? FilePath { get; set; }

    public bool IsEnabled => FilePath is { };
}
