using Luval.AuthMate.Core;
using SnapBite;
using System.Security.Claims;

namespace Luval.SnapBite.Fluent
{
    public static class HttpExtensions
    {
        public static bool IsAuthenticated(this IHttpContextAccessor c)
        {
            if (c == null) return false;
            return c != null && c.HttpContext != null && c.HttpContext.User != null && c.HttpContext.User.Identity != null &&
                c.HttpContext.User.Identity.IsAuthenticated;
        }


        public static bool IsPowerUser(this ClaimsPrincipal c)
        {
            return IsPowerUser((ClaimsIdentity)c.Identity);
        }

        public static bool IsPowerUser(this ClaimsIdentity c)
        {
            var users = ConfigHelper.GetValue<List<string>>("Authentication:PowerUsers");
            if (users == null) return false;
            var email = c.GetValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return false;
            return users.Select(i => i.ToLowerInvariant()).Contains(email.ToLowerInvariant());
        }
    }
}
