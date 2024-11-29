namespace Luval.SnapBite.Web
{
    public static class HttpExtensions
    {
        public static bool IsAuthenticated(this IHttpContextAccessor c)
        {
            if (c == null) return false;
            return c != null && c.HttpContext != null && c.HttpContext.User != null && c.HttpContext.User.Identity != null &&
                c.HttpContext.User.Identity.IsAuthenticated;
        }
    }
}
