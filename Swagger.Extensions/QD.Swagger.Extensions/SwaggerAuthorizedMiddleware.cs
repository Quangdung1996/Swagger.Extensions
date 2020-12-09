using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;

namespace QD.Swagger.Extensions
{
    internal class SwaggerAuthorizedMiddleware
    {
        readonly RequestDelegate _next;
        private readonly IHttpContextAccessor _httpContext;
        public SwaggerAuthorizedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            //Add your condition
            if (context.Request.Path.StartsWithSegments("/swagger")
                && !context.User.Identity.IsAuthenticated)
                await context.ChallengeAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            else
                await _next.Invoke(context);
        }
    }
}
