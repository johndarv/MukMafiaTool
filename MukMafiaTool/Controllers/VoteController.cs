namespace MukMafiaTool.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Mvc;
    using MukMafiaTool.Common;
    using MukMafiaTool.Database;

    public class VoteController : Controller
    {
        private IRepository repo;
        private VoteScanner voteScanner;

        public VoteController(IRepository repo)
        {
            this.repo = repo;
            this.voteScanner = new VoteScanner(this.repo);
        }

        // GET: Vote
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage RedetermineVotes()
        {
            this.repo.DeleteAllVotes();

            foreach (var post in this.repo.FindAllPosts())
            {
                foreach (var vote in this.voteScanner.ScanForVotes(post))
                {
                    this.repo.UpsertVote(vote);
                }
            }

            return HttpResponseMessageGenerator.GenerateOKMessage();
        }
    }
}