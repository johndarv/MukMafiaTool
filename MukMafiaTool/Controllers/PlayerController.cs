namespace MukMafiaTool.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;
    using MukMafiaTool.Common;
    using MukMafiaTool.Model;

    public class PlayerController : Controller
    {
        private IRepository repository;

        public PlayerController(IRepository repo)
        {
            this.repository = repo;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var players = this.repository.FindAllPlayers();

            return this.View(players.OrderBy(p => p.Name));
        }

        [HttpGet]
        public ActionResult Edit(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var player = this.repository.FindPlayer(playerName);

            if (player == null)
            {
                return this.HttpNotFound();
            }

            return this.View(player);
        }

        [HttpPost]
        [ActionName("Edit")]
        public ActionResult EditPost(Player player)
        {
            if (player == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            this.repository.UpsertPlayer(player);

            return this.View("index");
        }

        [HttpGet]
        public ActionResult ToggleParticipant(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var player = this.repository.FindPlayer(playerName);

            if (player == null)
            {
                return this.HttpNotFound();
            }

            player.Participating = player.Participating ? false : true;

            this.repository.UpsertPlayer(player);

            return this.RedirectToAction("index", "home");
        }

        [HttpGet]
        public ActionResult Kill(string playerName, string characterName, string roles, string fatality, string allegiance, string factionName)
        {
            var player = this.repository.FindPlayer(playerName);

            player.Character = characterName;
            player.Role = roles;
            player.Notes = string.Empty;
            player.Fatality = fatality;
            player.Recruitments.Clear();
            player.AddRecruitments(
                new List<Recruitment> { new Recruitment { Allegiance = (Allegiance)Enum.Parse(typeof(Allegiance), allegiance), FactionName = factionName, ForumPostNumber = "0" } });

            this.repository.UpsertPlayer(player);

            return this.RedirectToAction("index", "home");
        }
    }
}