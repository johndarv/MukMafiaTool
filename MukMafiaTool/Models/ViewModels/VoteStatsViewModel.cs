using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MukMafiaTool.Model;

namespace MukMafiaTool.Models.ViewModels
{
    public class VoteStatsViewModel
    {
        public int NumberOfVotes { get; set; }
        public double PercentageOfVotesOntoTown { get; set; }
        public double PercentageOfVotesOntoMafia { get; set; }
        public double PercentageOfVotesOntoNonTown { get; set; }

        public IEnumerable<IndividualVoteStatsViewModel> IndividualVoteStats { get; set; }
        public IEnumerable<FactionVoteStatsViewModel> FactionVoteStats { get; set; }
    }
}