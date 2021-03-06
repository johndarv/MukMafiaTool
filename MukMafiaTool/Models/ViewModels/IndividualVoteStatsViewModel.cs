﻿namespace MukMafiaTool.Models.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class IndividualVoteStatsViewModel
    {
        public string Name { get; set; }

        public int VotesCast { get; set; }

        public string FactionName { get; set; }

        public double PercentageOfVotesOntoTown { get; set; }

        public double PercentageOfVotesOntoNonTown { get; set; }

        public double PercentageOfVotesOntoMafia { get; set; }

        public double PercentageOfVotesOntoOther { get; set; }

        public double PercentageOfVotesOntoOwnAllegiance { get; set; }

        public double PercentageOfVotesOntoOwnFaction { get; set; }

        public string Character { get; internal set; }

        public int TimesVotedFor { get; set; }
    }
}
