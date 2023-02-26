namespace DripChip.WebApi.Configuration;

public sealed class SerilogSelfLogOptions
{
    public const string Position = "Serilog:SelfLog";

    public string? FilePath { get; set; }

    public bool IsEnabled => FilePath is { };
}
