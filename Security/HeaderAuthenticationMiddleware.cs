using System.Net;
using System.Security.Claims;

namespace MarketPlace.Security;

public class HeaderAuthenticationMiddleware : IMiddleware
{
    private readonly ILogger<HeaderAuthenticationMiddleware> _logger;

    public HeaderAuthenticationMiddleware(ILogger<HeaderAuthenticationMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Headers.TryGetValue("id", out var idHeader)
            || !context.Request.Headers.TryGetValue("email", out var emailHeader))
        {
            _logger.LogInformation("User doesn't have auth credentials");

            await next(context);
            return;
        }

        string? id = idHeader.FirstOrDefault();
        string? email = emailHeader.FirstOrDefault();
        if (id is null || !Guid.TryParse(id, out var idGuid) || email is null)
        {
            _logger.LogError("Invalid header credentials");
            context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
            
            return;
        }

        context.User = ExtractClaimsPrincipal(id, email);
        _logger.LogInformation("Extracted security claims to context");

        await next(context);
    }

    private static ClaimsPrincipal ExtractClaimsPrincipal(string id, string email)
    {
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, id),
            new Claim(ClaimTypes.Email, email),
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        return principal;
    }
}
