namespace MukMafiaTool.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Mvc;
    using MukMafiaTool.Common;
    using MukMafiaTool.Models.ViewModels;

    public class LogController : Controller
    {
        private IRepository repo;

        public LogController(IRepository repo)
        {
            this.repo = repo;
        }

        // GET: Log
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var messages = this.repo.FindAllLogMessages();
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
    }
}