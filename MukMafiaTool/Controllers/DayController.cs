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

        [HttpGet]
        public HttpResponseMessage RedetermineDays()
        {
            var posts = this.repository.FindAllPosts(includeDayZeros: true);
            var days = this.repository.FindAllDays();

            foreach (var post in posts)
            {
                var initialDay = post.Day;
                var redeterminedDay = post.DetermineDay(days);

                if (redeterminedDay != initialDay)
                {
                    post.Day = redeterminedDay;
                    this.repository.UpsertPost(post);
                }
            }

            return HttpResponseMessageGenerator.GenerateOKMessage();
        }

        [HttpPost]
        public HttpResponseMessage SetDay(int dayNumber, string startForumPostNumber, string endForumPostNumber)
        {
            if (endForumPostNumber == null)
            {
                endForumPostNumber = string.Empty;
            }

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