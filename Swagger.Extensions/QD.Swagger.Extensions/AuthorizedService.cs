using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace QD.Swagger.Extensions
{
    public class AuthorizedService : IAuthorizedService
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly string AccountClaimType = "UserInfo";

        public AuthorizedService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor;
        }

        public AuthorizedUser Get()
        {
            var item = _httpContext.HttpContext.User?.Claims
                .Where(c => c.Type.Equals(AccountClaimType)).Select(x => x.Value).FirstOrDefault();

            var result = JsonConvert.DeserializeObject<AuthorizedUser>(item);

            result.Role = _httpContext.HttpContext.User?.Claims
                .Where(c => c.Type.Equals(ClaimTypes.Role)).Select(x => x.Value).FirstOrDefault();

            return result;
        }

        public void SignIn(AuthorizedUser authorizedUser)
        {
            var userClaims = new List<Claim>()
                {
                    new Claim(AccountClaimType,JsonConvert.SerializeObject(authorizedUser)),
                    new Claim(ClaimTypes.Role, authorizedUser.Role),
                 };

            var claimsIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);

            _httpContext.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties());
        }
    }
}
