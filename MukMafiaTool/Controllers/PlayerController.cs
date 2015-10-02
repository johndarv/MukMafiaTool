using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MukMafiaTool.Common;

namespace MukMafiaTool.Controllers
{
    public class PlayerController : Controller
    {
        private IRepository _repo;

        public PlayerController(IRepository repo)
        {
            _repo = repo;
        }

        // GET: Player
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var player = _repo.FindPlayer(playerName);

            if (player == null)
            {
                return HttpNotFound();
            }

            return View(player);
        }
    }
}