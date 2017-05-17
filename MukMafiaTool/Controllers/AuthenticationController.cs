namespace MukMafiaTool.Controllers
{
    using System;
    using System.Security.Claims;
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.AspNet.Identity;
    using Microsoft.Owin.Security;
    using MukMafiaTool.Common;
    using MukMafiaTool.Model;

    public class AuthenticationController : Controller
    {
        private IRepository repo;

        public AuthenticationController(IRepository repo)
        {
            this.repo = repo;
        }

        public IAuthenticationManager Authentication
        {
            get { return this.HttpContext.GetOwinContext().Authentication; }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index()
        {
            return this.View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string userName, string password)
        {
            var user = this.Authenticate(userName, password);

            if (user != null)
            {
                var claims = new[] { new Claim(ClaimTypes.Name, userName) };

                var identity = new ClaimsIdentity(
                    claims,
                    DefaultAuthenticationTypes.ApplicationCookie,
                    ClaimTypes.Name,
                    ClaimTypes.Role);

                foreach (var role in user.Roles)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }

                //// Tell OWIN the identity provider, optional
                //// identity.AddClaim(new Claim(IdentityProvider, "Simplest Auth"));

                var properties = new AuthenticationProperties
                {
                    IsPersistent = false,
                };
                this.Authentication.SignIn(properties, identity);

                return this.RedirectToAction("index", "home");
            }

            return this.View("index", "Could not log you in");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Logout()
        {
            this.Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return this.RedirectToAction("index", "home");
        }

        private User Authenticate(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            var user = this.repo.FindUser(userName);

            if (user != null)
            {
                if (string.Equals(user.Password, password, StringComparison.Ordinal))
                {
                    return user;
                }
            }

            return null;
        }
    }
}