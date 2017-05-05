namespace MukMafiaTool.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;
    using MukMafiaTool.Common;

    public class PlayerController : Controller
    {
        private IRepository repo;

        public PlayerController(IRepository repo)
        {
            this.repo = repo;
        }

        // GET: Player
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var player = this.repo.FindPlayer(playerName);

            if (player == null)
            {
                return this.HttpNotFound();
            }

            return this.View(player);
        }
    }
}