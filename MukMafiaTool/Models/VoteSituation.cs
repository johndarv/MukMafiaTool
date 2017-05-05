namespace MukMafiaTool.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using MukMafiaTool.Model;

    public class VoteSituation
    {
        public IEnumerable<Vote> CurrentVotes { get; set; }

        public IEnumerable<Player> NotVoted { get; set; }
    }
}