using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;
using MukMafiaTool.Database;
using MukMafiaTool.Model;

namespace MukMafiaTool.Controllers
{
    public class PostController : Controller
    {
        private IRepository _repo;

        public PostController(IRepository repo)
        {
            _repo = repo;
        }

        // GET: Player
        public ActionResult Index(string playerName, string searchTerm, string forumPostNumber)
        {
            IList<ForumPost> posts = _repo.FindAllPosts();

            if (!string.IsNullOrEmpty(playerName))
            {
                posts = posts.Where(p => string.Equals(playerName, p.Poster, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                CultureInfo culture = CultureInfo.InvariantCulture;
                posts = posts.Where(p => culture.CompareInfo.IndexOf(p.Content.ToHtmlString(), searchTerm, CompareOptions.IgnoreCase) >= 0).ToList();
            }

            if (!string.IsNullOrEmpty(forumPostNumber))
            {
                posts = posts.Where(p => string.Equals(forumPostNumber, p.ForumPostNumber, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return View(posts);
        }
    }
}