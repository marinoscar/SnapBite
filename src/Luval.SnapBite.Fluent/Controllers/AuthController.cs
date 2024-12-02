using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Luval.SnapBite.Fluent.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("google-login")] //This mathches the configuration of the Google Auth
        public IActionResult GoogleLogin()
        {

            // Adds the properties ad redirect information
            // this could be change to include a redirect as part
            // of a query string if required
            var prop = new AuthenticationProperties()
            {
                RedirectUri = "/"
            };

            // Creates tthe challange
            var challange = Challenge(prop, GoogleDefaults.AuthenticationScheme);

            return challange;
        }

        [AllowAnonymous]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            // Signout the user from the OAuth flow
            await HttpContext.SignOutAsync();

            // Redirect to root so that when logging back in, it takes to home page
            return Redirect("/");
        }

    }
}
