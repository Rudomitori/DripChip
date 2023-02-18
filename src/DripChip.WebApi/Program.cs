using System.Text.Json.Serialization;
using DripChip.Domain.Accounts;
using DripChip.Domain.Exceptions;
using DripChip.Domain.PipelineBehaviors;
using DripChip.Domain.Utils;
using DripChip.Entities;
using DripChip.Persistence;
using DripChip.WebApi;
using DripChip.WebApi.Utils;
using FluentValidation;
using Hellang.Middleware.ProblemDetails;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using Serilog;
using JsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter(new UpperCaseJsonNamingPolicy())
    );
});

ProblemDetailsExtensions.AddProblemDetails(
    builder.Services,
    options =>
    {
        options.IncludeExceptionDetails = (ctx, ex) => builder.Environment.IsDevelopment();

        options.MapToStatusCode<NotFoundException>(StatusCodes.Status404NotFound);
        options.MapToStatusCode<ForbiddenException>(StatusCodes.Status403Forbidden);
        options.MapToStatusCode<UnauthorizedException>(StatusCodes.Status401Unauthorized);
        options.MapToStatusCode<ConflictException>(StatusCodes.Status409Conflict);
        options.MapToStatusCode<InternalException>(StatusCodes.Status500InternalServerError);

        options.Map<ValidationException>(
            (ctx, ex) =>
            {
                var factory = ctx.RequestServices.GetRequiredService<ProblemDetailsFactory>();

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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "http",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Enter email and password",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "basic",
        }
    );
    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "http"
                    },
                    Scheme = "basic",
                    Name = "basic",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        }
    );
});

builder.Host.UseSerilog(
    (context, provider, configuration) =>
    {
        configuration.ReadFrom.Services(provider).ReadFrom.Configuration(context.Configuration);
    }
);

builder.Services.AddValidatorsFromAssembly(typeof(CreateAccount).Assembly);
builder.Services.AddMediatR(typeof(CreateAccount));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehaviour<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehaviour<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehaviour<,>));

builder.Services.AddIdentityCore<Account>().AddEntityFrameworkStores<AppDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 1;
});

builder.Services
    .AddAuthentication("Basic")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null);

builder.Services.AddAuthorization();

builder.Services.AddDbContext<AppDbContext>(optionsBuilder =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default");
    optionsBuilder.UseNpgsql(
        connectionString,
        contextOptionsBuilder =>
        {
            contextOptionsBuilder.UseNetTopologySuite();
        }
    );
});
builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());

builder.Services.AddSingleton(
    new NtsGeometryServices(
        CoordinateArraySequenceFactory.Instance,
        new PrecisionModel(1000d),
        4326, // longitude and latitude
        GeometryOverlay.NG,
        new CoordinateEqualityComparer()
    )
);

builder.Services.AddSingleton<IClock>(new Clock(TimeSpan.TicksPerMillisecond));

var app = builder.Build();

using (var serviceScope = app.Services.CreateScope())
{
    var appDbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
    await appDbContext.Database.EnsureDeletedAsync();
    await appDbContext.Database.EnsureCreatedAsync();
}

app.MapHealthChecks("/health");

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (context, httpContext) =>
    {
        context.Set("RemoteIp", httpContext.Connection.RemoteIpAddress);
    };
});

app.UseProblemDetails();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
