namespace MukMafiaTool.Controllers
{
    using System;
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

        [HttpPost]
        public ActionResult Edit(Player player)
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

        [HttpPost]
        public ActionResult Kill(Player player)
        {
            throw new NotImplementedException();
        }
    }
}