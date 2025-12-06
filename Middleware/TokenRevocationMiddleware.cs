using Kantin_Paramadina.Model;
using Microsoft.EntityFrameworkCore;

namespace Kantin_Paramadina.Middleware
{
    public class TokenRevocationMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenRevocationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
        {
            // skip cek token untuk login
            if (context.Request.Path.StartsWithSegments("/api/auth/login") || context.Request.Path.StartsWithSegments("/api/auth/register"))
            {
                await _next(context);
                return;
            }
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                var tokenExists = await db.UserToken
                    .AnyAsync(t => t.Token == token && !t.Revoked && t.ExpiredAt > DateTime.UtcNow);

                if (!tokenExists)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Token invalid or revoked.");
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Authorization header missing.");
                return;
            }

            await _next(context);
        }
    }
}
