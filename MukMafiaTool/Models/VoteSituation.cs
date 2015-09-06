using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MukMafiaTool.Model;

namespace MukMafiaTool.Models
{
    public class VoteSituation
    {
        public IEnumerable<Vote> CurrentVotes { get; set; }
        public IEnumerable<Player> NotVoted { get; set; }
    }
}