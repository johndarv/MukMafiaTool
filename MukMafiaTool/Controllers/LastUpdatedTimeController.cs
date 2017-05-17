namespace MukMafiaTool.Controllers
{
    using System;
    using System.Web.Mvc;
    using MukMafiaTool.Database;

    public class LastUpdatedTimeController : Controller
    {
        private readonly MongoRepository repository;

        public LastUpdatedTimeController()
        {
            this.repository = new MongoRepository();
        }

        [HttpGet]
        public ActionResult SetLastUpdatedTime(DateTime dateTime)
        {
            this.repository.UpdateLastUpdatedTime(dateTime);

            return this.RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult SetLastUpdateTimeToThreeMonthsAgo()
        {
            this.repository.UpdateLastUpdatedTime(DateTime.UtcNow.AddMonths(-3));

            return this.RedirectToAction("Index", "Home");
        }
    }
}
