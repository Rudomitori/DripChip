using Common.Core.Clock;
using DripChip.Domain.Accounts;
using DripChip.Domain.PipelineBehaviors;
using FluentValidation;
using MediatR;

namespace DripChip.WebApi.Setup;

public static class DomainSetup
{
    public static WebApplicationBuilder SetupDomain(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssembly(typeof(CreateAccount).Assembly);
        builder.Services.AddMediatR(typeof(CreateAccount));
        builder.Services.AddScoped(
            typeof(IPipelineBehavior<,>),
            typeof(LoggingPipelineBehaviour<,>)
        );
        builder.Services.AddScoped(
            typeof(IPipelineBehavior<,>),
            typeof(ExceptionHandlingBehaviour<,>)
        );
        builder.Services.AddScoped(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationPipelineBehaviour<,>)
        );

        builder.Services.AddSingleton<IClock>(new Clock(TimeSpan.TicksPerMillisecond));

        return builder;
    }
}
