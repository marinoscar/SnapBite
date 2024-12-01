using Luval.AuthMate.Entities;
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

        public static AppUser ToUser(this ClaimsPrincipal identity)
        {
            return ((ClaimsIdentity)identity.Identity).ToUser();
        }

        public static AppUser ToUser(this ClaimsIdentity identity)
        {
            return new AppUser()
            {
                ProviderType = identity.GetValue("providerName"),
                ProviderKey = identity.GetValue(ClaimTypes.NameIdentifier),
                DisplayName = identity.GetValue(ClaimTypes.Name),
                Email = identity.GetValue(ClaimTypes.Email),
                ProfilePictureUrl = identity.GetValue("urn:google:image")
            };
        }

        public static string? GetValue(this ClaimsIdentity c, string type)
        {
            if (c == null) return null;
            if (!c.HasClaim(i => i.Type == type)) return null;
            return c.Claims.First(i => i.Type == type).Value;
        }

        public static string? GetValue(this ClaimsPrincipal c, string type)
        {
            return ((ClaimsIdentity)c.Identity).GetValue(type);
        }
    }
}
