namespace MukMafiaTool.Controllers
{
    using System.Net;
    using System.Net.Http;
    using System.Web.Mvc;
    using MukMafiaTool.Common;

    public class DayController : Controller
    {
        private IRepository repo;

        public DayController(IRepository repo)
        {
            this.repo = repo;
        }

        // GET: Day
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage RedetermineDays()
        {
            var posts = this.repo.FindAllPosts(includeDayZeros: true);

            foreach (var post in posts)
            {
                post.Day = post.DetermineDay(this.repo);
                this.repo.UpsertPost(post);
            }

            var response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }
    }
}