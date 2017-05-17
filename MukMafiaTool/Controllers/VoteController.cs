namespace MukMafiaTool.Controllers
{
    using System.Linq;
    using System.Net.Http;
    using System.Web.Mvc;
    using MukMafiaTool.Common;
    using MukMafiaTool.Model;

    public class VoteController : Controller
    {
        private IRepository repository;
        private VoteScanner voteScanner;

        public VoteController(IRepository repo)
        {
            this.repository = repo;
            this.voteScanner = new VoteScanner(this.repository);
        }

        [HttpGet]
        public HttpResponseMessage RedetermineVotes()
        {
            this.repository.DeleteAllVotes();

            foreach (var post in this.repository.FindAllPosts())
            {
                foreach (var vote in this.voteScanner.ScanForVotes(post))
                {
                    this.repository.UpsertVote(vote);
                }
            }

            return HttpResponseMessageGenerator.GenerateOKMessage();
        }

        [HttpGet]
        public ActionResult Index()
        {
            var votes = this.repository.FindAllVotes();

            votes = votes.OrderBy(v => long.Parse(v.ForumPostNumber));

            return this.View(votes);
        }

        [HttpGet]
        public ActionResult Edit(string forumPostNumber, int postContentIndex)
        {
            var vote = this.repository.FindVote(forumPostNumber, postContentIndex);

            return this.View("Edit", vote);
        }

        [HttpPost]
        [ActionName("Edit")]
        public ActionResult EditPost(Vote vote)
        {
            this.repository.UpsertVote(vote, overrideManuallyEditedVotes: true);

            return this.RedirectToAction("Index", "Vote");
        }
    }
}