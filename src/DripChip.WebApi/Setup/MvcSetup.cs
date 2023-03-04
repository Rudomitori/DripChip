using System.Text.Json.Serialization;
using Common.Core.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace DripChip.WebApi.Setup;

public static class MvcSetup
{
    public static WebApplicationBuilder SetupControllers(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHealthChecks();
        builder.Services.Configure<JsonOptions>(options =>
        {
            options.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter(new UpperCaseJsonNamingPolicy())
            );
        });

        return builder;
    }

    public static void UseControllersSetup(this WebApplication app)
    {
        app.MapControllers();
    }

    public const string HealthCheckRoute = "/health";

    public static void UseHealthCheckSetup(this WebApplication app)
    {
        app.MapHealthChecks(HealthCheckRoute);
    }

    public static WebApplicationBuilder SetupSwagger(this WebApplicationBuilder builder)
    {
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

        return builder;
    }

    public static void UseSwaggerSetup(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
}
