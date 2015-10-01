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

        public VoteController(IRepository repo)
        {
            _repo = repo;
        }

        // GET: Vote
        public HttpResponseMessage ReDetermineVotes()
        {
            _repo.DeleteAllVotes();

            var players = _repo.FindAllPlayers();
            var playerNameGroups = players.Select(p => (new string[] { p.Name }).Concat(p.Aliases));

            foreach (var post in _repo.FindAllPosts())
            {
                foreach (var vote in VoteScanner.ScanForVotes(post, playerNameGroups))
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