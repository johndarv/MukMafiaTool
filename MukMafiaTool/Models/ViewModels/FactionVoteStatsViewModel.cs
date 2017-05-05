namespace MukMafiaTool.Models.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MukMafiaTool.Model;

    public class FactionVoteStatsViewModel
    {
        public string Name { get; set; }

        public int VotesCast { get; set; }

        public Allegiance Allegiance { get; set; }

        public double PercentageOfVotesOntoOwnFaction { get; set; }
    }
}
