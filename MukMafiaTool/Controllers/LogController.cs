namespace MukMafiaTool.Controllers
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Web.Mvc;
    using MukMafiaTool.Common;
    using MukMafiaTool.Models.ViewModels;

    public class LogController : Controller
    {
        private IRepository repository;

        public LogController(IRepository repo)
        {
            this.repository = repo;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var messages = this.repository.FindAllLogMessages();
            IList<LogMessage> logMessages = new List<LogMessage>();

            var postNumberRegex = new Regex("Forum Post Number: [0-9]+");
            var numbersRegex = new Regex("[0-9]+");

            foreach (var message in messages)
            {
                var logMessage = new LogMessage();
                logMessage.Message = message;

                var postNumberMatch = postNumberRegex.Match(message);

                if (postNumberMatch.Success)
                {
                    var numbersMatch = numbersRegex.Match(postNumberMatch.Value);

                    logMessage.ForumPostNumber = numbersMatch.Value;
                }

                logMessages.Add(logMessage);
            }

            return this.View(logMessages);
        }

        [HttpGet]
        public ActionResult DeleteAll()
        {
            this.repository.DeleteAllLogMessages();

            return this.RedirectToAction("Index");
        }
    }
}