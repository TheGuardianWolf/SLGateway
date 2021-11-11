using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace SLWatchtower.Controllers
{
    [Controller]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        [Route("login")]
        public async Task Login(string returnUrl = "/")
        {
            await HttpContext.ChallengeAsync("Auth0", new AuthenticationProperties() { RedirectUri = returnUrl });
        }

        [Route("signup")]
        public async Task SignUp(string returnUrl = "/")
        {
            var authProperties = new AuthenticationProperties() { RedirectUri = returnUrl };
            await HttpContext.ChallengeAsync("Auth0", authProperties);

            // Modify redirect to signup
            var uriBuilder = new UriBuilder(Response.Headers["Location"]);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["screen_hint"] = "signup";
            uriBuilder.Query = query.ToString();
            uriBuilder.Port = -1;
            Response.Headers["Location"] = uriBuilder.ToString();
        }

        [Route("logout")]
        [HttpPost]
        [Authorize]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Auth0", new AuthenticationProperties
            {
                // Indicate here where Auth0 should redirect the user after a logout.
                // Note that the resulting absolute Uri must be added to the
                // **Allowed Logout URLs** settings for the app.
                RedirectUri = Url.Action("Index", "Home")
            });
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
