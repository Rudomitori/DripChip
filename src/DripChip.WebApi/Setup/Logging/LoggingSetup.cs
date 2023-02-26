using Common.Core.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Debugging;
using Serilog.Sinks.Elasticsearch;

namespace DripChip.WebApi.Setup.Logging;

public static class LoggingSetup
{
    public static WebApplicationBuilder SetupLogging(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<OpenSearchLoggingOptions>(
            builder.Configuration.GetSection(OpenSearchLoggingOptions.Position)
        );

        builder.Services.Configure<SerilogSelfLogOptions>(
            builder.Configuration.GetSection(SerilogSelfLogOptions.Position)
        );

        var serilogSelfLogOptions = builder.Configuration.Create<SerilogSelfLogOptions>();

        if (serilogSelfLogOptions.IsEnabled)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(serilogSelfLogOptions.FilePath!)!);

            var streamWriter = File.Exists(serilogSelfLogOptions.FilePath)
                ? new StreamWriter(File.OpenWrite(serilogSelfLogOptions.FilePath))
                : File.CreateText(serilogSelfLogOptions.FilePath!);

            SelfLog.Enable(TextWriter.Synchronized(streamWriter));
        }

        builder.Host.UseSerilog(
            (context, provider, configuration) =>
            {
                configuration.ReadFrom
                    .Services(provider)
                    .ReadFrom.Configuration(context.Configuration);

                var openSearchLoggingOptions = provider
                    .GetRequiredService<IOptions<OpenSearchLoggingOptions>>()
                    .Value;
                if (openSearchLoggingOptions.IsEnabled)
                {
                    var elasticsearchSinkOptions = new ElasticsearchSinkOptions(
                        new Uri(openSearchLoggingOptions.Uri!)
                    )
                    {
                        BatchPostingLimit = openSearchLoggingOptions.BatchPostingLimit,
                        AutoRegisterTemplate = true,
                        TemplateName = openSearchLoggingOptions.Template,
                        IndexFormat =
                            $"{openSearchLoggingOptions.Template!.ToLower()}--{{0:yyyy.MM.dd}}",
                        RegisterTemplateFailure = RegisterTemplateRecovery.FailSink,
                        TypeName = null,
                        EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
                    };

                    elasticsearchSinkOptions.ModifyConnectionSettings = c =>
                    {
                        c.BasicAuthentication(
                            openSearchLoggingOptions.User,
                            openSearchLoggingOptions.Password
                        );

                        // Quick fix of error with self signed certificates
                        // Source: https://github.com/serilog-contrib/serilog-sinks-elasticsearch/issues/384#issuecomment-861645387
                        if (openSearchLoggingOptions.SkipSslCheck)
                            c.ConnectionLimit(-1)
                                .ServerCertificateValidationCallback((_, _, _, _) => true);

                        return c;
                    };

                    configuration.WriteTo.Elasticsearch(elasticsearchSinkOptions);
                }
            }
        );

        return builder;
    }

    public static void UseLoggingSetup(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (context, httpContext) =>
            {
                context.Set("RemoteIp", httpContext.Connection.RemoteIpAddress);
            };
        });
    }
}
