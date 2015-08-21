using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MukMafiaTool.Database;
using MukMafiaTool.Models;
using MukMafiaTool.Models.ViewModels;

namespace MukMafiaTool.Controllers
{
    public class HomeController : Controller
    {
        private IRepository _repo;

        public HomeController(IRepository repo)
        {
            _repo = repo;
        }

        public ActionResult Index()
        {
            HomeViewModel viewModel = new HomeViewModel();

            var allPosts = _repo.FindAllPosts();
            var postGroups = allPosts.GroupBy(p => p.Poster);
            viewModel.Players = postGroups.Select(g => new Player() { Name = g.First().Poster, PostCount = g.Count() }).ToList();

            var playerNames = postGroups.Select(g => new SelectListItem() { Value = g.First().Poster, Text = g.First().Poster }).ToList();
            playerNames.Add(new SelectListItem() { Value = string.Empty, Text = string.Empty });
            viewModel.PlayerNames = playerNames.ToArray();

            var allVotes = _repo.FindAllVotes();
            viewModel.Votes = allVotes.GroupBy(v => v.Recipient).OrderByDescending(g => g.Count());

            foreach (var voteGroup in viewModel.Votes)
            {
                var name = voteGroup.Key;
                var count = voteGroup.Count();
                var list = string.Join(", ", voteGroup.Select(v => v.Voter).ToArray());
            }

            var voted = allVotes.Select(v => v.Voter);
            var exclusions = _repo.FindAllExclusions();
            var notVoted = viewModel.Players.Where(p => !voted.Any(v => string.Equals(v, p.Name, StringComparison.OrdinalIgnoreCase))).ToList();
            notVoted = notVoted.Where(p => !exclusions.Any(e => string.Equals(e, p.Name, StringComparison.OrdinalIgnoreCase))).ToList();
            viewModel.NotVoted = notVoted.OrderBy(p => p.PostCount).ToList();

            viewModel.LastUpdated = _repo.FindLastUpdatedDateTime();

            viewModel.Days = _repo.FindAllDays();

            return View(viewModel);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}