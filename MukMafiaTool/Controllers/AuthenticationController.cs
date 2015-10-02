using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using MukMafiaTool.Common;

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
            if (Authenticate(userName, password))
            {
                var claims = new[] { new Claim(ClaimTypes.Name, userName) };

                var identity = new ClaimsIdentity(
                    claims,
                    DefaultAuthenticationTypes.ApplicationCookie,
                    ClaimTypes.Name, ClaimTypes.Role);


                identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                
                // Tell OWIN the identity provider, optional
                // identity.AddClaim(new Claim(IdentityProvider, "Simplest Auth"));

                Authentication.SignIn(new AuthenticationProperties
                {
                    IsPersistent = false,
                }, identity);

                return RedirectToAction("index", "home");
            }

            return View("Show", userName);
        }

        public ActionResult Logout()
        {
            Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("index", "home");
        }

        private bool Authenticate(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            var user = _repo.FindUser(userName);

            if (user != null)
            {
                return string.Equals(user.Password, password, StringComparison.Ordinal);
            }
            
            return false;
        }
    }
}