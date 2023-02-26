using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Common.Domain.Exceptions;
using DripChip.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DripChip.WebApi;

public sealed class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    #region Constructor and dependencies

    private readonly UserManager<Account> _userManager;

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        UserManager<Account> userManager
    )
        : base(options, logger, encoder, clock)
    {
        _userManager = userManager;
    }

    #endregion

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Response.Headers.Add("WWW-Authenticate", "Basic");

        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeaders))
            return AuthenticateResult.NoResult();

        foreach (var authorizationHeader in authorizationHeaders)
        {
            if (authorizationHeader is null || !authorizationHeader.StartsWith("Basic "))
                continue;

            string userName;
            string password;
            try
            {
                var base64String = authorizationHeader.Substring("Basic ".Length);
                var decodedBytes = Convert.FromBase64String(base64String);
                var decodedString = Encoding.UTF8.GetString(decodedBytes);
                var splitString = decodedString.Split(':', 2);
                userName = splitString[0];
                password = splitString[1];
            }
            catch (Exception e)
            {
                return AuthenticateResult.Fail("Authorization code not formatted properly.");
            }

            var account = await _userManager.Users.FirstOrDefaultAsync(
                x => x.NormalizedEmail == _userManager.NormalizeEmail(userName)
            );

            var verified =
                account is { }
                && _userManager.PasswordHasher.VerifyHashedPassword(
                    account,
                    account.PasswordHash,
                    password
                ) is PasswordVerificationResult.Success;

            if (!verified)
                throw new UnauthorizedException("The username or password is not correct.");

            // if (!verified)
            //     return AuthenticateResult.Fail("The username or password is not correct.");

            var identity = new ClaimsIdentity(account.GetClaims(), Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
        }

        return AuthenticateResult.NoResult();
    }
}
