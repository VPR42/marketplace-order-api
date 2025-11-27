using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace MarketPlace.Security;

public class HeaderAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public HeaderAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("id", out var idHeader)
            || !Request.Headers.TryGetValue("email", out var emailHeader))
        {
            Logger.LogInformation("User doesn't have auth credentials");

            return Task.FromResult(AuthenticateResult.NoResult());
        }

        string? id = idHeader.FirstOrDefault();
        string? email = emailHeader.FirstOrDefault();
        if (id is null || !Guid.TryParse(id, out var idGuid) || email is null)
        {
            Logger.LogError("Invalid header credentials");
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
        }

        var principal = ExtractClaimsPrincipal(id, email);
        Logger.LogInformation("Extracted security claims to context");

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }

    private ClaimsPrincipal ExtractClaimsPrincipal(string id, string email)
    {
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, id),
            new Claim(ClaimTypes.Email, email),
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);

        return principal;
    }
}