using DripChip.Entities;
using DripChip.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace DripChip.WebApi.Setup.Auth;

public static class AuthSetup
{
    public static WebApplicationBuilder SetupAuth(this WebApplicationBuilder builder)
    {
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

        builder.Services.AddAuthorization(options =>
        {
            NotAuthorizedAttribute.AddPolicy(options);
        });

        return builder;
    }

    public static void UseAuthSetup(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }
}
