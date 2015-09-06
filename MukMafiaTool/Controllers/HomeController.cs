using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MukMafiaTool.Database;
using MukMafiaTool.Models;
using MukMafiaTool.Models.ViewModels;
using MukMafiaTool.Votes;

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
            var players = _repo.FindAllPlayers();
            var postGroups = allPosts.GroupBy(p => p.Poster);
            var days = _repo.FindAllDays();

            // Get all players to display and their post counts and so forth
            viewModel.Players = players.Select(player =>
                {
                    return new HomePagePlayer
                    {
                        Name = player.Name,
                        PostCount = postGroups.Single(g => g.Key == player.Name).Count(),
                        IsInGame = player.Participating && string.IsNullOrEmpty(player.Fatality),
                        Character = player.Character,
                        OutOfGameText = !string.IsNullOrEmpty(player.Fatality) ? player.Fatality : player.Notes,
                    };
                });

            // Enumerate the list of player names for the drop down menu
            var playerNames = players.Select(player => new SelectListItem() { Value = player.Name, Text = player.Name }).ToList();
            playerNames.Add(new SelectListItem() { Value = string.Empty, Text = string.Empty });
            viewModel.PlayerNames = playerNames.OrderBy(p => p.Text).ToArray();

            // Find all current votes
            viewModel.VoteSituation = VoteAnalyser.DetermineCurrentVoteSituation(_repo.FindAllVotes(), players, days);

            viewModel.LastUpdated = _repo.FindLastUpdatedDateTime();

            viewModel.Days = days;

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