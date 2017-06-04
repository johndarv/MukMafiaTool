namespace MukMafiaTool.Controllers
{
    using System;
    using System.Web.Mvc;
    using MukMafiaTool.Database;
    using MukMafiaTool.ForumScanning;

    public class ScanController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index()
        {
            var message = string.Empty;

            using (var repository = new MongoRepository())
            {
                try
                {
                    var lastUpdatedTime = repository.FindLastUpdatedDateTime();

                    var timeSinceLastUpdate = DateTime.UtcNow.Subtract(lastUpdatedTime);

                    if (timeSinceLastUpdate > TimeSpan.FromMinutes(4))
                    {
                        ForumScanner scanner = new ForumScanner(repository);
                        scanner.DoWholeUpdate();

                        message = "Scan was successful.";
                    }
                    else
                    {
                        message = $"Did not scan because the previous scan was only {timeSinceLastUpdate.TotalMinutes.ToString("F1")} minutes ago.";
                    }
                }
                catch (Exception e)
                {
                    message = $"Error: Could not scan the forum.";

                    repository.LogMessage($"Error scanning forum: {e.Message}");
                }
            }

            TempData["HomepageMessage"] = message;

            return RedirectToAction("index", "home");
        }
    }
}