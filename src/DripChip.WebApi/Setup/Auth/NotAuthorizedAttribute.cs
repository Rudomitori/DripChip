using Microsoft.AspNetCore.Authorization;

namespace DripChip.WebApi.Setup.Auth;

public class NotAuthorizedAttribute : AuthorizeAttribute
{
    public const string Policy = "NotAuthorized";

    public NotAuthorizedAttribute()
        : base(Policy) { }

    public static void AddPolicy(AuthorizationOptions options) =>
        options.AddPolicy(
            Policy,
            builder =>
                builder.RequireAssertion(context => context.User.Identity?.IsAuthenticated is false)
        );
}
