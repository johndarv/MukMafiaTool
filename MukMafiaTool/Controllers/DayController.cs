namespace MukMafiaTool.Controllers
{
    using System.Net;
    using System.Net.Http;
    using System.Web.Mvc;
    using MukMafiaTool.Common;
    using MukMafiaTool.Model;

    public class DayController : Controller
    {
        private IRepository repository;

        public DayController(IRepository repo)
        {
            this.repository = repo;
        }

        // GET: Day
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage RedetermineDays()
        {
            var posts = this.repository.FindAllPosts(includeDayZeros: true);

            foreach (var post in posts)
            {
                post.Day = post.DetermineDay(this.repository);
                this.repository.UpsertPost(post);
            }

            return HttpResponseMessageGenerator.GenerateOKMessage();
        }

        [Authorize(Roles = "Admin")]
        public HttpResponseMessage SetDay(int dayNumber, string startForumPostNumber, string endForumPostNumber)
        {
            var day = new Day
            {
                Number = dayNumber,
                StartForumPostNumber = startForumPostNumber,
                EndForumPostNumber = endForumPostNumber,
            };

            this.repository.UpsertDay(day);

            return HttpResponseMessageGenerator.GenerateOKMessage();
        }
    }
}