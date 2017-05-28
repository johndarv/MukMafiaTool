namespace MukMafiaTool.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;
    using MukMafiaTool.Common;
    using MukMafiaTool.Model;
    using MukMafiaTool.Models;
    using MukMafiaTool.Models.ViewModels;
    using MukMafiaTool.Votes;

    public class HomeController : Controller
    {
        private IRepository repo;

        public HomeController(IRepository repo)
        {
            this.repo = repo;
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Message = DetermineHomepageMessage();

            HomeViewModel viewModel = new HomeViewModel();

            var players = this.repo.FindAllPlayers();
            var days = this.repo.FindAllDays();

            // Get all players to display and their post counts and so forth
            viewModel.Players = this.DeterminePlayers(players);

            // Enumerate the list of player names for the drop down menu
            var playerNames = players.Select(player => new SelectListItem() { Value = player.Name, Text = player.Name }).ToList();
            playerNames.Add(new SelectListItem() { Value = string.Empty, Text = string.Empty });
            viewModel.PlayerNames = playerNames.OrderBy(p => p.Text).ToArray();

            // Find all current votes
            viewModel.VoteSituation = VoteAnalyser.DetermineCurrentVoteSituation(this.repo.FindAllVotes(), players, days);

            viewModel.LastUpdated = this.repo.FindLastUpdatedDateTime();

            viewModel.Days = days;

            return this.View(viewModel);
        }

        private IEnumerable<HomepagePlayer> DeterminePlayers(IEnumerable<Model.Player> players)
        {
            var allPosts = this.repo.FindAllPosts();
            var postGroups = allPosts.GroupBy(p => p.Poster);

            // filter out players not participating who aren't the Games Master/Moderator
            players = players.Where(p => p.Participating || string.Equals(p.Role, "Games Master") || string.Equals(p.Role, "Moderator"));

            var homePagePlayers = players.Select(player => this.ToHomePagePlayer(player, postGroups));

            // filter out players with 0 posts
            var zeroPosterFilter = ConfigurationManager.AppSettings["FilterZeroPosters"];

            if (!string.IsNullOrEmpty(zeroPosterFilter) && string.Equals("true", zeroPosterFilter, StringComparison.OrdinalIgnoreCase))
            {
                homePagePlayers = homePagePlayers.Where(p => p.PostCount > 0);
            }

            return homePagePlayers;
        }

        private HomepagePlayer ToHomePagePlayer(Player player, IEnumerable<IGrouping<string, ForumPost>> postGroups)
        {
            var posts = postGroups.SingleOrDefault(g => g.Key == player.Name);

            var isInGame = player.Participating && string.IsNullOrEmpty(player.Fatality);

            var factionNames = player.Recruitments.OrderBy(r => r.ForumPostNumber).Select(r => r.FactionName);
            var factions = string.Join(", ", factionNames);

            var homePagePlayer = new HomepagePlayer
            {
                Name = player.Name,
                PostCount = posts != null ? posts.Count() : 0,
                IsInGame = isInGame,
            };

            if (isInGame == false)
            {
                homePagePlayer.Character = player.Character;
                homePagePlayer.Role = player.Role;
                homePagePlayer.Fatality = player.Fatality;
                homePagePlayer.Factions = factions;
            }

            return homePagePlayer;
        }

        private string DetermineHomepageMessage()
        {
            var appSettingsMessage = ConfigurationManager.AppSettings["HomepageMessage"];
            var tempDataMessage = TempData["HomepageMessage"];

            if (appSettingsMessage != null)
            {
                return appSettingsMessage;
            }
            else if (tempDataMessage != null)
            {
                return tempDataMessage.ToString();
            }

            return string.Empty;
        }
    }
}