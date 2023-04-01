namespace DripChip.WebApi.Setup.Logging;

public sealed class OpenSearchLoggingOptions 
{
    public static string Position => "Serilog:OpenSearch";
    public string? Uri { get; set; }
    public string? Template { get; set; }
    public string? User { get; set; }
    public string? Password { get; set; }

    public int BatchPostingLimit { get; set; } = 50;

    public bool SkipSslCheck { get; set; }

    public bool IsEnabled => Uri is { } && Template is { };
}
