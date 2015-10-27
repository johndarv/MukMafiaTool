using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MukMafiaTool.Models.ViewModels
{
    public class IndividualVoteStatsViewModel
    {
        public string Name { get; set; }
        public int VotesCast { get; set; }
        public double PercentageOfVotesOntoTown { get; set; }
        public double PercentageOfVotesOntoNonTown { get; set; }
        public double PercentageOfVotesOntoMafia { get; set; }
        public double PercentageOfVotesOntoOther { get; set; }
        public double PercentageOfVotesOntoOwnAllegiance { get; set; }
        public double PercentageOfVotesOntoOwnFaction { get; set; }
        
        public int TimesVotedFor { get; set; }
    }
}
