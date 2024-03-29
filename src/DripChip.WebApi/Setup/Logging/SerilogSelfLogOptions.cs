using Common.Core.Configuration;

namespace DripChip.WebApi.Setup.Logging;

public sealed class SerilogSelfLogOptions
{
    public static string Position => "Serilog:SelfLog";
    public string? FilePath { get; set; }

    public bool IsEnabled => FilePath is { };
}
