using MukMafiaTool.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MukMafiaTool.Controllers
{
    public class LogController : Controller
    {
        private IRepository _repo;

        public LogController(IRepository repo)
        {
            _repo = repo;
        }

        // GET: Log
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(_repo.FindAllLogMessages());
        }
    }
}