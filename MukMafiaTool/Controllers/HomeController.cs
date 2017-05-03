using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
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

            // filter out players not participating who aren't the Games Master/Moderator
            players = players.Where(p => p.Participating || string.Equals(p.Notes, "Games Master") || string.Equals(p.Notes, "Moderator"));

            var homePagePlayers = players.Select(player => ToHomePagePlayer(player, postGroups));

            // filter out players with 0 posts
            var zeroPosterFilter = ConfigurationManager.AppSettings["FilterZeroPosters"];
            
            if (!string.IsNullOrEmpty(zeroPosterFilter) && string.Equals("true", zeroPosterFilter, StringComparison.OrdinalIgnoreCase))
            {
                homePagePlayers = homePagePlayers.Where(p => p.PostCount > 0);
            }

            return homePagePlayers;
        }

        private HomePagePlayer ToHomePagePlayer(Player player, IEnumerable<IGrouping<string, ForumPost>> postGroups)
        {
            var posts = postGroups.SingleOrDefault(g => g.Key == player.Name);

            var isInGame = player.Participating && string.IsNullOrEmpty(player.Fatality);

            var extraInfoBuilder = new StringBuilder();
            
            if (!isInGame)
            {
                if (!string.IsNullOrEmpty(player.Character))
                {
                    extraInfoBuilder.Append($" - {player.Character}");
                }

                if (player.Recruitments.Any())
                {
                    var factionNames = player.Recruitments.OrderBy(r => r.ForumPostNumber).Select(r => r.FactionName);

                    extraInfoBuilder.Append($", {string.Join(", ", factionNames)}");
                }

                if (!string.IsNullOrEmpty(player.Fatality))
                {
                    extraInfoBuilder.Append($" - {player.Fatality}");
                }

                if (!string.IsNullOrEmpty(player.Notes))
                {
                    extraInfoBuilder.Append($" - {player.Notes}");
                }
            }

            return new HomePagePlayer
            {
                Name = player.Name,
                PostCount = posts != null ? posts.Count() : 0,
                IsInGame = isInGame,
                ExtraInfo = extraInfoBuilder.ToString(),
            };
        }
    }
}