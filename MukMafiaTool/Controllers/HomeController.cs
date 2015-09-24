using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MukMafiaTool.Common;
using MukMafiaTool.Database;
using MukMafiaTool.Model;
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

            var players = _repo.FindAllPlayers();
            var days = _repo.FindAllDays();

            // Get all players to display and their post counts and so forth
            viewModel.Players = DeterminePlayers(players);

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

        private IEnumerable<HomePagePlayer> DeterminePlayers(IEnumerable<Model.Player> players)
        {
            var allPosts = _repo.FindAllPosts();
            var postGroups = allPosts.GroupBy(p => p.Poster);

            // filter out players not participating who aren't the Games Master
            players = players.Where(p => p.Participating || string.Equals(p.Notes, "Games Master"));

            var homePagePlayers = players.Select(player => ToHomePagePlayer(player, postGroups));

            // filter out players with 0 posts
            homePagePlayers = homePagePlayers.Where(p => p.PostCount > 0);

            return homePagePlayers;
        }

        private HomePagePlayer ToHomePagePlayer(Player player, IEnumerable<IGrouping<string, ForumPost>> postGroups)
        {
            var posts = postGroups.SingleOrDefault(g => g.Key == player.Name);

            return new HomePagePlayer
            {
                Name = player.Name,
                PostCount = posts != null ? posts.Count() : 0,
                IsInGame = player.Participating && string.IsNullOrEmpty(player.Fatality),
                Character = player.Character,
                OutOfGameText = !string.IsNullOrEmpty(player.Fatality) ? player.Fatality : player.Notes,
            };
        }
    }
}