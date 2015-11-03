using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using MukMafiaTool.Common;
using MukMafiaTool.Database;

namespace MukMafiaTool.Controllers
{
    public class VoteController : Controller
    {
        private IRepository _repo;
        private VoteScanner _voteScanner;

        public VoteController(IRepository repo)
        {
            _repo = repo;
            _voteScanner = new VoteScanner(_repo);
        }

        // GET: Vote
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage ReDetermineVotes()
        {
            _repo.DeleteAllVotes();
            
            foreach (var post in _repo.FindAllPosts())
            {
                foreach (var vote in _voteScanner.ScanForVotes(post))
                {
                    _repo.UpsertVote(vote);
                }
            }

            var response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }
    }
}