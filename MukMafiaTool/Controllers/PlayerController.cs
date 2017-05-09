namespace MukMafiaTool.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web;
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

        // GET: Player
        [Authorize(Roles = "Admin")]
        public ActionResult Index(string playerName)
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

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(Player player)
        {
            if (player == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            this.repository.UpsertPlayer(player);

            return this.View("index");
        }

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
        public ActionResult Kill(string playerName)
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
    }
}