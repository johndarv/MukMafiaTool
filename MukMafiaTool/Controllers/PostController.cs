﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using MukMafiaTool.Common;
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
        public ActionResult Index(string playerName, string searchString, string forumPostNumber, bool includeQuoteBlocks = false)
        {
            var posts = _repo.FindAllPosts();

            if (!string.IsNullOrEmpty(playerName))
            {
                posts = posts.Where(p => string.Equals(playerName, p.Poster, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                posts = FilterOnSearchString(searchString, includeQuoteBlocks, posts.ToList());
            }

            if (!string.IsNullOrEmpty(forumPostNumber))
            {
                posts = posts.Where(p => string.Equals(forumPostNumber, p.ForumPostNumber, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return View(posts.ToList());
        }

        private static IList<ForumPost> FilterOnSearchString(string searchString, bool includeQuoteBlocks, IList<ForumPost> posts)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;

            var searchTerms = searchString.SplitOnSeparatorExceptInSpeechMarks(' ');

            posts = posts.Where(
                (p) =>
                {
                    string content = includeQuoteBlocks ? p.Content.ToHtmlString() : p.Content.ToHtmlString().FilterOutContentInQuoteBlocks();
                    var plainText = content.StripHtml(true);

                    bool include = true;

                    foreach (var searchTerm in searchTerms)
                    {
                        if (culture.CompareInfo.IndexOf(plainText, searchTerm.Trim('\"'), CompareOptions.IgnoreCase) < 0)
                        {
                            include = false;
                        }
                    }

                    return include;
                })
                .ToList();

            return posts;
        }
    }
}