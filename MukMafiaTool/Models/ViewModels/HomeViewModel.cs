using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MukMafiaTool.Database;
using MukMafiaTool.Model;

namespace MukMafiaTool.Models.ViewModels
{
    public class HomeViewModel
    {
        public IList<Player> Players { get; set; }
        public IEnumerable<SelectListItem> PlayerNames { get; set; }
        public IEnumerable<IGrouping<string, Vote>> Votes { get; set; }
        public IList<Player> NotVoted { get; set; }
        public DateTime LastUpdated { get; set; }
        public IList<Day> Days { get; set; }
    }
}