namespace MukMafiaTool.Models.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using MukMafiaTool.Database;
    using MukMafiaTool.Model;

    public class HomeViewModel
    {
        public IEnumerable<HomePagePlayer> Players { get; set; }

        public IEnumerable<SelectListItem> PlayerNames { get; set; }

        public VoteSituation VoteSituation { get; set; }

        public DateTime LastUpdated { get; set; }

        public IList<Day> Days { get; set; }

        public Exception Error { get; set; }
    }
}