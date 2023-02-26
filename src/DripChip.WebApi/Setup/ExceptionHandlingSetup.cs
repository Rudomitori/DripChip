using Common.Domain.Exceptions;
using FluentValidation;
using Hellang.Middleware.ProblemDetails;

namespace DripChip.WebApi.Setup;

public static class ExceptionHandlingSetup
{
    public static WebApplicationBuilder SetupExceptionHandling(this WebApplicationBuilder builder)
    {
        ProblemDetailsExtensions.AddProblemDetails(
            builder.Services,
            options =>
            {
                options.IncludeExceptionDetails = (ctx, ex) => builder.Environment.IsDevelopment();

                options.MapToStatusCode<NotFoundException>(StatusCodes.Status404NotFound);
                options.MapToStatusCode<ForbiddenException>(StatusCodes.Status403Forbidden);
                options.MapToStatusCode<UnauthorizedException>(StatusCodes.Status401Unauthorized);
                options.MapToStatusCode<ConflictException>(StatusCodes.Status409Conflict);
                options.MapToStatusCode<InternalException>(
                    StatusCodes.Status500InternalServerError
                );

                options.Map<ValidationException>(
                    (ctx, ex) =>
                    {
                        var factory =
                            ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                        var errors = ex.Errors
                            .GroupBy(x => x.PropertyName)
                            .ToDictionary(x => x.Key, x => x.Select(x => x.ErrorMessage).ToArray());

                        return factory.CreateValidationProblemDetails(
                            ctx,
                            errors,
                            StatusCodes.Status400BadRequest
                        );
                    }
                );
            }
        );

        return builder;
    }

    public static void UseExceptionHandlingSetup(this WebApplication app)
    {
        app.UseProblemDetails();
    }
}
