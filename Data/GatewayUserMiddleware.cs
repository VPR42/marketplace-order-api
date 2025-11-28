using System.Security.Claims;

namespace MarketPlace.Data
{
    public class GatewayUserMiddleware
    {
        private readonly RequestDelegate _next;

        public GatewayUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var userId = context.Request.Headers["id"].FirstOrDefault();
            var email = context.Request.Headers["email"].FirstOrDefault();

            if (userId != null)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email ?? "")
            };

                var identity = new ClaimsIdentity(claims, "GatewayAuth");
                context.User = new ClaimsPrincipal(identity);
            }

            await _next(context);
        }
    }

}
