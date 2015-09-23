using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using MukMafiaTool.Database;
using MukMafiaTool.Common;

namespace MukMafiaTool.Controllers
{
    public class DayController : Controller
    {
        IRepository _repo;

        public DayController(IRepository repo)
        {
            _repo = repo;
        }

        // GET: Day
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage RecalculateDays()
        {
            var posts = _repo.FindAllPosts(includeDayZeros: true);
            var days = _repo.FindAllDays();

            foreach (var post in posts)
            {
                post.Day = post.DetermineDay(days);
                _repo.UpsertPost(post);
            }
            
            var response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }
    }
}