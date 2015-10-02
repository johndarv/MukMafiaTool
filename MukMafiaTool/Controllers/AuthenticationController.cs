using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using MukMafiaTool.Common;
using MukMafiaTool.Model;

namespace MukMafiaTool.Controllers
{
    public class AuthenticationController : Controller
    {
        private IRepository _repo;

        IAuthenticationManager Authentication
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        public AuthenticationController(IRepository repo)
        {
            _repo = repo;
        }

        // GET: Authentication
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login(string userName, string password)
        {
            var user = Authenticate(userName, password);

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
                
                // Tell OWIN the identity provider, optional
                // identity.AddClaim(new Claim(IdentityProvider, "Simplest Auth"));

                var properties = new AuthenticationProperties
                {
                    IsPersistent = false,
                };
                Authentication.SignIn(properties, identity);

                return RedirectToAction("index", "home");
            }

            return View("index", "Could not log you in");
        }

        public ActionResult Logout()
        {
            Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("index", "home");
        }

        private User Authenticate(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            var user = _repo.FindUser(userName);

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